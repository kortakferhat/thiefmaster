using UnityEngine;
using UnityEngine.InputSystem;
using Gameplay.Graph;
using Gameplay.Events;
using Infrastructure.Managers.LevelManager;
using Infrastructure.Managers;
using TowerClicker.Infrastructure;
using Infrastructure;
using DG.Tweening;

namespace Gameplay.Character
{
    public class CharacterController : BaseEntity
    {
        [Header("Components")]
        [SerializeField] private Transform characterTransform;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationDuration = 0.2f;
        [SerializeField] private Ease rotationEase = Ease.OutQuad;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private InputSystem_Actions _playerInputActions;
        private Vector2Int _currentNodeId;
        private ILevelManager _levelManager;
        private ITurnManager _turnManager;
        private bool _isMoving = false;
        private Tween _currentMoveTween;
        
        // Interface properties
        public Vector2Int CurrentNodeId => _currentNodeId;
        public bool IsMoving => _isMoving;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _levelManager = ServiceLocator.Get<ILevelManager>();
            _turnManager = ServiceLocator.Get<ITurnManager>();
            
            _levelManager.OnLevelLoaded += OnLevelLoaded;
            _levelManager.OnGridInstantiated += OnGridInstantiated;
            
            InitializeInputSystem();
        }
        
        private void InitializeInputSystem()
        {
            _playerInputActions = new InputSystem_Actions();
            _playerInputActions.Player.Move.performed += OnMovementPerformed;
            _playerInputActions.Player.Enable();
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
            _isMoving = false;
            
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
        
        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            if (_isMoving) return;
            if (_turnManager.IsTurnInProgress) return; // Prevent input during turn processing
            
            var input = context.ReadValue<Vector2>();
            var direction = GetDirectionFromInput(input);
            
            if (direction != Vector2Int.zero)
            {
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
                    Debug.Log($"[CharacterController] Cannot move to {targetNodeId} - Enemy occupied!");
                    EventBus.Publish(new LoseEvent(_turnManager.CurrentTurn, LoseReason.EnemyContact, targetNodeId));
                    return;
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
                _currentMoveTween.Kill();
            }
            
            _isMoving = true;
            var targetPos = _levelManager.GetNodeActualWorldPosition(_currentNodeId);
            var targetRotation = GetRotationFromDirection(direction);
            
            _currentMoveTween = DOTween.Sequence()
                .Insert(0f, characterTransform.DORotate(targetRotation, rotationDuration).SetEase(rotationEase))
                .Insert(0f, characterTransform.DOMove(targetPos, moveDuration).SetEase(moveEase))
                .OnComplete(OnMoveComplete)
                .SetAutoKill(true);
        }
        
        private void OnMoveComplete()
        {
            _isMoving = false;
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
                EventBus.Publish(new WinEvent(_turnManager.CurrentTurn, _currentNodeId));
                
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
            
            _playerInputActions.Player.Move.performed -= OnMovementPerformed;
            _playerInputActions.Player.Disable();
            _playerInputActions.Dispose();
            
            _levelManager.OnLevelLoaded -= OnLevelLoaded;
            _levelManager.OnGridInstantiated -= OnGridInstantiated;
        }
    }
}
