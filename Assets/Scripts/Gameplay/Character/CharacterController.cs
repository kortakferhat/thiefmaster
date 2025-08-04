using UnityEngine;
using UnityEngine.InputSystem;
using Gameplay.Graph;
using Infrastructure.Managers.LevelManager;
using TowerClicker.Infrastructure;

namespace Gameplay.Character
{
    public class CharacterController : BaseEntity
    {
        private const string LOG_TAG = "[CharacterController]";
        
        [Header("Components")]
        [SerializeField] private Transform characterTransform;
        
        private InputSystem_Actions _playerInputActions;
        private Vector2Int _currentNodeId;
        private ILevelManager _levelManager;
        
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
            Debug.Log($"{LOG_TAG} Started at node: {_currentNodeId}");
        }
        
        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Vector2Int direction = GetDirectionFromInput(input);
            
            Debug.Log($"{LOG_TAG} Input received: {input} -> Direction: {direction}");
            
            if (direction != Vector2Int.zero)
            {
                TryMoveToNode(direction);
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
        
        private void TryMoveToNode(Vector2Int direction)
        {
            if (_levelManager.TryMoveToNode(_currentNodeId, direction, out Vector2Int targetNodeId))
            {
                var oldNodeId = _currentNodeId;
                _currentNodeId = targetNodeId;
                UpdateCharacterPosition();
                Debug.Log($"{LOG_TAG} Moved from {oldNodeId} to {_currentNodeId} via direction {direction}");
            }
            else
            {
                Debug.Log($"{LOG_TAG} Cannot move from {_currentNodeId} in direction {direction} - invalid move");
            }
        }
        
        private void UpdateCharacterPosition()
        {
            var worldPos = _levelManager.GetNodeActualWorldPosition(_currentNodeId);
            transform.position = worldPos;
            Debug.Log($"{LOG_TAG} Position updated to: {worldPos} (Node: {_currentNodeId})");
        }
        
        private void OnDestroy()
        {
            _playerInputActions.Player.Move.performed -= OnMovementPerformed;
            _playerInputActions.Player.Disable();
            _playerInputActions.Dispose();
            
            _levelManager.OnLevelLoaded -= OnLevelLoaded;
            _levelManager.OnGridGenerated -= OnGridGenerated;
        }
    }
}
