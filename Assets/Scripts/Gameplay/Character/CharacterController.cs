using UnityEngine;
using UnityEngine.InputSystem;
using Gameplay.Graph;
using Gameplay.Events;
using Infrastructure.Managers.LevelManager;
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
        
        private InputSystem_Actions _playerInputActions;
        private Vector2Int _currentNodeId;
        private ILevelManager _levelManager;
        private ITurnManager _turnManager;
        private bool _isMoving = false;
        private Tween _currentMoveTween;
        
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
        
        private void OnLevelLoaded(GraphScriptableObject levelGraph)
        {
            // Graph is now handled by LevelManager
        }
        
        private void OnGridInstantiated(Graph.Graph graph)
        {
            SetStartPosition(graph.GetStartNode());
        }
        
        private void SetStartPosition(Node startNode)
        {
            _currentNodeId = startNode.Id;
            UpdateCharacterPosition();
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
            
            // Complete the turn after movement animation finishes
            _turnManager.CompleteTurn();
            
            Debug.Log($"[CharacterController] Player movement completed at node {_currentNodeId}, Turn {_turnManager.CurrentTurn} finished");
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
