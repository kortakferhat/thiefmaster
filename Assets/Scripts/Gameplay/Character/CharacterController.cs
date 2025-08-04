using UnityEngine;
using UnityEngine.InputSystem;
using Gameplay.Graph;
using Infrastructure.Managers.LevelManager;
using TowerClicker.Infrastructure;
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
        
        private InputSystem_Actions _playerInputActions;
        private Vector2Int _currentNodeId;
        private ILevelManager _levelManager;
        private bool _isMoving = false;
        private Tween _currentMoveTween;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _levelManager = ServiceLocator.Get<ILevelManager>();
            _levelManager.OnLevelLoaded += OnLevelLoaded;
            _levelManager.OnGridGenerated += OnGridGenerated;
            
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
        
        private void OnGridGenerated(Graph.Graph graph)
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
                _currentNodeId = targetNodeId;
                AnimateToPosition();
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
        
        private void AnimateToPosition()
        {
            // Kill previous tween if it exists
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill();
            }
            
            _isMoving = true;
            var targetPos = _levelManager.GetNodeActualWorldPosition(_currentNodeId);
            
            _currentMoveTween = characterTransform.DOMove(targetPos, moveDuration)
                .SetEase(moveEase)
                .OnComplete(OnMoveComplete)
                .SetAutoKill(true);
        }
        
        private void OnMoveComplete()
        {
            _isMoving = false;
            _currentMoveTween = null;
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
            
            _playerInputActions.Player.Move.performed -= OnMovementPerformed;
            _playerInputActions.Player.Disable();
            _playerInputActions.Dispose();
            
            _levelManager.OnLevelLoaded -= OnLevelLoaded;
            _levelManager.OnGridGenerated -= OnGridGenerated;
        }
    }
}
