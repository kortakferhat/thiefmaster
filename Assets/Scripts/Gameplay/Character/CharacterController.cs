using UnityEngine;
using UnityEngine.InputSystem;
using Gameplay.Graph;
using Gameplay.Events;
using Infrastructure.Managers.LevelManager;
using Infrastructure;
using DG.Tweening;

namespace Gameplay.Character
{
    public enum CharacterState : int
    {
        Dead = -1,
        Idle = 0,
        Walk = 1,
        Attacking = 2,
        Defending = 3,
        Win = 4,
    }
    
    public class CharacterController : BaseEntity
    {
        // Interface properties
        public Vector2Int CurrentNodeId => _currentNodeId;
        public CharacterState CurrentState => _currentState;
        
        [Header("Components")]
        [SerializeField] private Transform characterTransform;
        [SerializeField] private Animator animator;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveDuration = 1f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationDuration = 0.25f;
        [SerializeField] private Ease rotationEase = Ease.OutQuad;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        [Header("Input Components")]
        [SerializeField] private Infrastructure.Input.SwipeInputHandler swipeInputHandler;
        
        private InputSystem_Actions _playerInputActions;
        private Vector2Int _currentNodeId;
        private ILevelManager _levelManager;
        private ITurnManager _turnManager;
        private Tween _currentMoveTween;
        private CharacterState _currentState = CharacterState.Idle;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _levelManager = ServiceLocator.Get<ILevelManager>();
            _turnManager = ServiceLocator.Get<ITurnManager>();
            
            _levelManager.OnLevelLoaded += OnLevelLoaded;
            _levelManager.OnGridInstantiated += OnGridInstantiated;
            
            EventBus.Subscribe<GameEvents.GameStateChangeEvent>(OnGameStateChangeEvent);
            
            InitializeInputSystem();
        }
        
        private void InitializeInputSystem()
        {
            // Initialize Unity Input System
            _playerInputActions = new InputSystem_Actions();
            _playerInputActions.Player.Move.performed += OnMovementInputPerformed;
            _playerInputActions.Player.Enable();
            
            // Initialize Swipe Input Handler
            if (swipeInputHandler != null)
            {
                swipeInputHandler.OnSwipeDetected.AddListener(OnSwipeDetected);
                if (showDebugLogs)
                    Debug.Log("[CharacterController] Swipe input handler initialized");
            }
            else
            {
                Debug.LogWarning("[CharacterController] SwipeInputHandler component not assigned!");
            }
        }
        
        private void OnGridInstantiated(Graph.Graph graph)
        {
            SetStartPosition(graph.GetStartNode());
        }
        
        private void OnLevelLoaded(GraphScriptableObject levelGraph)
        {
            // Reset character to start position when level is restarted
            var graph = _levelManager.GetCurrentGraph();
            if (graph != null)
            {
                var startNode = graph.GetStartNode();
                if (startNode != null)
                {
                    SetStartPosition(startNode);
                }
            }
        }
        
        private void SetStartPosition(Node startNode)
        {
            // Cancel any ongoing movement animations
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill();
                _currentMoveTween = null;
            }
            
            // Reset movement state
            SetCurrentState(CharacterState.Idle);
            
            // Update current node ID
            _currentNodeId = startNode.Id;
            
            // Immediately update character position without animation
            UpdateCharacterPosition();
            
            // Reset character rotation to default (facing up)
            characterTransform.rotation = Quaternion.identity;
            
            if (showDebugLogs)
                Debug.Log($"[CharacterController] Character reset to start position at node {_currentNodeId}");
        }
        
        /// <summary>
        /// Reset character's movement state and position (can be called externally)
        /// </summary>
        public void ResetToStartPosition()
        {
            var graph = _levelManager.GetCurrentGraph();
            if (graph != null)
            {
                var startNode = graph.GetStartNode();
                if (startNode != null)
                {
                    SetStartPosition(startNode);
                }
            }
        }
        
        private void OnMovementInputPerformed(InputAction.CallbackContext context)
        {
            if (_currentState is not CharacterState.Idle) return;
            if (_turnManager.IsTurnInProgress) return; // Prevent input during turn processing
            if (_gameManager.State != GameState.Game)  return;
            
            var input = context.ReadValue<Vector2>();
            var direction = GetDirectionFromInput(input);
            
            if (direction != Vector2Int.zero)
            {
                TryMoveToNode(direction);
            }
        }
        
        private void OnSwipeDetected(Infrastructure.Input.SwipeDirection swipeDirection)
        {
            if (_currentState is not CharacterState.Idle) return;
            if (_turnManager.IsTurnInProgress) return; // Prevent input during turn processing
            if (_gameManager.State != GameState.Game) return;
            
            var direction = GetDirectionFromSwipe(swipeDirection);
            
            if (direction != Vector2Int.zero)
            {
                if (showDebugLogs)
                    Debug.Log($"[CharacterController] Swipe detected: {swipeDirection} -> Direction: {direction}");
                    
                TryMoveToNode(direction);
            }
        }
        
        private void TryMoveToNode(Vector2Int direction)
        {
            if (_levelManager.TryMoveToNode(_currentNodeId, direction, out Vector2Int targetNodeId))
            {
                // Check if target node is occupied by enemy
                var enemyManager = ServiceLocator.Get<IGridEnemyManager>();
                
                if (enemyManager != null && enemyManager.IsNodeOccupiedByEnemy(targetNodeId))
                {
                    var enemy = enemyManager.GetEnemyAtNode(targetNodeId);
                    var enemyDirection = enemy.FacingDirection;
                    var characterDirection = direction;
                    var shouldLost = enemyDirection == -characterDirection;

                    if (shouldLost)
                    {
                        Debug.Log($"[CharacterController] Cannot move to {targetNodeId} - Enemy occupied!");
                        _gameManager.LoseGame();
                        return;
                    }
                }
                
                var previousNodeId = _currentNodeId;
                _currentNodeId = targetNodeId;
                
                // Start turn and trigger events
                _turnManager.StartNextTurn();
                EventBus.Publish(new PlayerMovedEvent(previousNodeId, _currentNodeId, _turnManager.CurrentTurn));
                
                AnimateToPosition(direction);
            }
        }
        
        private Vector2Int GetDirectionFromInput(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                return input.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }
        
        private Vector2Int GetDirectionFromSwipe(Infrastructure.Input.SwipeDirection swipeDirection)
        {
            switch (swipeDirection)
            {
                case Infrastructure.Input.SwipeDirection.Up:
                    return Vector2Int.up;
                case Infrastructure.Input.SwipeDirection.Down:
                    return Vector2Int.down;
                case Infrastructure.Input.SwipeDirection.Left:
                    return Vector2Int.left;
                case Infrastructure.Input.SwipeDirection.Right:
                    return Vector2Int.right;
                default:
                    return Vector2Int.zero;
            }
        }
        
        private Vector3 GetRotationFromDirection(Vector2Int direction)
        {
            if (direction == Vector2Int.right)
                return new Vector3(0, 90, 0);
            else if (direction == Vector2Int.left)
                return new Vector3(0, -90, 0);
            else if (direction == Vector2Int.up)
                return new Vector3(0, 0, 0);
            else if (direction == Vector2Int.down)
                return new Vector3(0, 180, 0);
            
            return Vector3.zero;
        }
        
        private void AnimateToPosition(Vector2Int direction)
        {
            // Kill previous tween if it exists
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill(true);
            }
            
            SetCurrentState(CharacterState.Walk);
            var targetPos = _levelManager.GetNodeActualWorldPosition(_currentNodeId);
            var targetRotation = GetRotationFromDirection(direction);
            
            _currentMoveTween = DOTween.Sequence()
                .Insert(0f, characterTransform.DORotate(targetRotation, rotationDuration).SetEase(rotationEase))
                .Insert(0f, characterTransform.DOMove(targetPos, moveDuration).SetEase(moveEase))
                .InsertCallback(moveDuration * .75f, () =>
                {
                    if (_currentState is not CharacterState.Dead)
                    {
                        SetAnimationState(CharacterState.Idle);
                    }
                    
                }).SetEase(moveEase)
                .OnComplete(OnMoveComplete)
                .SetAutoKill(true);
        }
        
        private void OnMoveComplete()
        {
            SetCurrentState(CharacterState.Idle);
            _currentMoveTween = null;
            
            // Check if player reached goal node
            CheckWinCondition();
            
            // Complete the turn after movement animation finishes
            _turnManager.CompleteTurn();
            
            Debug.Log($"[CharacterController] Player movement completed at node {_currentNodeId}, Turn {_turnManager.CurrentTurn} finished");
        }
        
        /// <summary>
        /// Check if player has reached the goal node
        /// </summary>
        private void CheckWinCondition()
        {
            var graph = _levelManager.GetCurrentGraph();
            var goalNode = graph.GetGoalNode();
            
            if (goalNode != null && _currentNodeId == goalNode.Id)
            {
                Debug.Log($"[CharacterController] Player reached goal node {_currentNodeId} - Level Complete!");
                _gameManager.WinGame();
                //EventBus.Publish(new WinEvent(_turnManager.CurrentTurn, _currentNodeId));
                
                // Notify LevelManager
                _levelManager.CompleteLevel();
            }
        }
        
        private void UpdateCharacterPosition()
        {
            var worldPos = _levelManager.GetNodeActualWorldPosition(_currentNodeId);
            characterTransform.position = worldPos;
        }
        
        private void OnDestroy()
        {
            // Kill any active tween
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill();
            }
            _currentMoveTween = null;
            
            // Kill all tweens on this transform to be safe
            characterTransform.DOKill();
            
            // Clean up input systems
            if (_playerInputActions != null)
            {
                _playerInputActions.Player.Move.performed -= OnMovementInputPerformed;
                _playerInputActions.Player.Disable();
                _playerInputActions.Dispose();
            }
            
            // Clean up swipe input handler
            if (swipeInputHandler != null)
            {
                swipeInputHandler.OnSwipeDetected.RemoveListener(OnSwipeDetected);
            }
            
            _levelManager.OnLevelLoaded -= OnLevelLoaded;
            _levelManager.OnGridInstantiated -= OnGridInstantiated;
            
            EventBus.Unsubscribe<GameEvents.GameStateChangeEvent>(OnGameStateChangeEvent);
        }

        private void OnGameStateChangeEvent(GameEvents.GameStateChangeEvent args)
        {
            if (args.CurrentState == GameState.Finish)
            {
                if (args.Reason == GameEvents.GameEventChangeReason.Lose)
                {
                    SetCurrentState(CharacterState.Dead);
                }
            }
        }

        private void SetCurrentState(CharacterState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            SetAnimationState(_currentState);

            if (showDebugLogs)
                Debug.Log($"[CharacterController] State changed to {_currentState}");
        }

        private void SetAnimationState(CharacterState state)
        {
            var stateName = state.ToString().ToLower();
            animator.SetTrigger(stateName);
        }
    }
}
