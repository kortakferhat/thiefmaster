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
        
        private InputSystem_Actions _playerInputActions;
        private Vector2Int _currentNodeId;
        private Graph.Graph _currentGraph;
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
            _currentGraph = levelGraph.CreateGraph();
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
                // Map Unity input to graph coordinates
                // Unity: up = (0,1), down = (0,-1)
                // Graph: up = (0,1), down = (0,-1) - now correct
                return input.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }
        
        private void TryMoveToNode(Vector2Int direction)
        {
            var targetNodeId = _currentNodeId + direction;
            var targetNode = _currentGraph.GetNode(targetNodeId);
            
            Debug.Log($"{LOG_TAG} Current node: {_currentNodeId}, Direction: {direction}, Target node: {targetNodeId}, Found: {targetNode != null}");
            
            if (targetNode != null)
            {
                // Check if there's a valid edge between current and target node
                var currentNode = _currentGraph.GetNode(_currentNodeId);
                var edge = _currentGraph.GetEdge(currentNode, direction);
                
                if (edge != null)
                {
                    var oldNodeId = _currentNodeId;
                    _currentNodeId = targetNodeId;
                    UpdateCharacterPosition();
                    Debug.Log($"{LOG_TAG} Moved from {oldNodeId} to {_currentNodeId} via direction {direction}");
                }
                else
                {
                    Debug.Log($"{LOG_TAG} Cannot move from {_currentNodeId} to {targetNodeId} - no valid edge");
                }
            }
            else
            {
                Debug.Log($"{LOG_TAG} Cannot move from {_currentNodeId} in direction {direction} - target node does not exist");
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
