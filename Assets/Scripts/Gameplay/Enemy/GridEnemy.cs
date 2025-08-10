using UnityEngine;
using Gameplay.Graph;
using Gameplay.Events;
using Infrastructure.Managers.LevelManager;
using Infrastructure.Managers;
using TowerClicker.Infrastructure;
using Infrastructure;

namespace Gameplay.Enemy
{
    /// <summary>
    /// Grid-based stationary guard enemy for the graph puzzle game
    /// </summary>
    public class GridEnemy : BaseEntity, IPoolable
    {
        [Header("Enemy Settings")]
        [SerializeField] private Transform enemyTransform;
        [SerializeField] private Vector2Int facingDirection = Vector2Int.up;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private Vector2Int _currentNodeId;
        private ITurnManager _turnManager;
        private ILevelManager _levelManager;
        private bool _isInitialized = false;
        private Vector2Int _currentPlayerNodeId; // Track player position
        
        public Vector2Int CurrentNodeId => _currentNodeId;
        public Vector2Int FacingDirection => facingDirection;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _turnManager = ServiceLocator.Get<ITurnManager>();
            _levelManager = ServiceLocator.Get<ILevelManager>();
            
            // Get initial player position from start node
            var graph = _levelManager.GetCurrentGraph();
            var startNode = graph.GetStartNode();
            if (startNode != null)
            {
                _currentPlayerNodeId = startNode.Id;
            }
            
            // Subscribe to turn events
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            
            _isInitialized = true;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Initialized at node {_currentNodeId}, facing {facingDirection}, player at {_currentPlayerNodeId}");
        }
        
        /// <summary>
        /// Set the enemy's position on the grid
        /// </summary>
        public void SetPosition(Vector2Int nodeId)
        {
            _currentNodeId = nodeId;
            
            var worldPos = _levelManager.GetNodeActualWorldPosition(_currentNodeId);
            enemyTransform.position = worldPos;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Positioned at node {_currentNodeId}");
        }
        
        /// <summary>
        /// Move enemy to a new position (for future movement implementation)
        /// </summary>
        public void MoveToNode(Vector2Int newNodeId)
        {
            var previousNodeId = _currentNodeId;
            
            // Update position
            SetPosition(newNodeId);
            
            // Update graph: previous node becomes normal, new node occupied by enemy
            var graph = _levelManager.GetCurrentGraph();
            // Note: We don't set new node to Enemy type since that's for spawn points only
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Moved from {previousNodeId} to {newNodeId}");
        }
        
        /// <summary>
        /// Set the enemy's facing direction
        /// </summary>
        public void SetFacingDirection(Vector2Int direction)
        {
            facingDirection = direction;
            UpdateVisualRotation();
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Now facing {facingDirection}");
        }
        
        private void UpdateVisualRotation()
        {
            // Convert direction to rotation
            Vector3 rotation = GetRotationFromDirection(facingDirection);
            enemyTransform.rotation = Quaternion.Euler(rotation);
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
        
        private void OnTurnStarted(TurnStartedEvent turnEvent)
        {
            if (!_isInitialized) return;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Turn {turnEvent.TurnNumber} started - Enemy is alert");
            
            // Enemy behavior during turn start
            // For now, just log - we'll add vision detection later
        }
        
        private void OnPlayerMoved(PlayerMovedEvent moveEvent)
        {
            if (!_isInitialized) return;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Player moved from {moveEvent.FromNodeId} to {moveEvent.ToNodeId} on turn {moveEvent.TurnNumber}");
            
            // Update current player node
            _currentPlayerNodeId = moveEvent.ToNodeId;

            // Check if player is in vision range (1 depth)
            CheckPlayerDetection(_currentPlayerNodeId);
            
            // Enemy moves after player movement
            PerformEnemyMovement();
        }
        
        private void CheckPlayerDetection(Vector2Int playerNodeId)
        {
            // Check if player is on the same node as enemy
            if (playerNodeId == _currentNodeId)
            {
                Debug.Log($"[GridEnemy] Player reached enemy node {_currentNodeId} - Enemy eliminated!");
                RemoveThisEnemy();
                return;
            }
            
            // Check if player is in vision range (1 depth)
            if (IsPlayerInVision(playerNodeId))
            {
                Debug.Log($"[GridEnemy] Player spotted at {playerNodeId} - Game Over!");
                // Trigger game over event
                EventBus.Publish(new GameOverEvent(_turnManager.CurrentTurn));
            }
        }
        
        /// <summary>
        /// Check if player position is within enemy's vision (1 depth, facing direction)
        /// Only sees along valid edges, not through walls
        /// </summary>
        private bool IsPlayerInVision(Vector2Int playerPosition)
        {
            // Get current graph to check valid connections
            var graph = _levelManager.GetCurrentGraph();
            
            // Calculate the node we're looking at (1 depth in facing direction)
            var visionNode = _currentNodeId + facingDirection;
            
            // Player is in vision if:
            // 1. They're at the node we're looking at
            // 2. There's a valid edge connection to that node
            if (playerPosition == visionNode)
            {
                // Check if there's a valid path from enemy to vision node
                return graph.CanMoveFromTo(_currentNodeId, visionNode);
            }
            
            return false;
        }
        
        /// <summary>
        /// Enemy moves in facing direction, or reverses direction if blocked
        /// </summary>
        private void PerformEnemyMovement()
        {
            var graph = _levelManager.GetCurrentGraph();
            var targetNode = _currentNodeId + facingDirection;
            
            // Check if we can move in facing direction
            if (graph.CanMoveFromTo(_currentNodeId, targetNode))
            {
                // Check if player is at target node
                if (IsPlayerAtNode(targetNode))
                {
                    // Player is at target node - move there and trigger game over
                    MoveToNode(targetNode);
                    EventBus.Publish(new GameOverEvent(_turnManager.CurrentTurn));
                    return;
                }
                
                // Move to target node
                MoveToNode(targetNode);
            }
            else
            {
                // Cannot move in facing direction, reverse direction
                ReverseFacingDirection();
                
                // Try to move in new direction
                var newTargetNode = _currentNodeId + facingDirection;
                if (graph.CanMoveFromTo(_currentNodeId, newTargetNode))
                {
                    // Check if player is at new target node
                    if (IsPlayerAtNode(newTargetNode))
                    {
                        // Player is at target node - move there and trigger game over
                        MoveToNode(newTargetNode);
                        EventBus.Publish(new GameOverEvent(_turnManager.CurrentTurn));
                        return;
                    }
                    
                    // Move to new target node
                    MoveToNode(newTargetNode);
                }
            }
        }
        
        /// <summary>
        /// Reverse the enemy's facing direction (180 degree turn)
        /// </summary>
        private void ReverseFacingDirection()
        {
            facingDirection = -facingDirection;
            UpdateVisualRotation();
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Reversed direction, now facing {facingDirection}");
        }
        
        /// <summary>
        /// Check if player is at the specified node
        /// </summary>
        private bool IsPlayerAtNode(Vector2Int nodeId)
        {
            // Use stored player position from event
            return _currentPlayerNodeId == nodeId;
        }
        
        /// <summary>
        /// Remove this enemy from the game
        /// </summary>
        private void RemoveThisEnemy()
        {
            // Publish enemy eliminated event
            EventBus.Publish(new EnemyEliminatedEvent(_currentNodeId, _currentNodeId, _turnManager.CurrentTurn));
            
            // Get GridEnemyManager and remove this enemy
            var enemyManager = ServiceLocator.Get<IGridEnemyManager>();
            enemyManager.RemoveEnemy(this);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
        }
        
        #region IPoolable Implementation
        
        public void OnSpawnFromPool()
        {
            // Reset enemy state when spawned from pool
            _isInitialized = false;
            _currentPlayerNodeId = Vector2Int.zero; // Reset player position
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Spawned from pool");
        }
        
        public void OnDespawn()
        {
            // Clean up when returning to pool
            _isInitialized = false;
            _currentPlayerNodeId = Vector2Int.zero; // Reset player position
            
            // Unsubscribe from events to prevent memory leaks
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Returning to pool");
        }
        
        #endregion
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !_isInitialized) return;
            
            // Draw enemy position
            var worldPos = _levelManager.GetNodeActualWorldPosition(_currentNodeId);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(worldPos, 0.3f);
            
            // Draw facing direction
            Gizmos.color = Color.yellow;
            var facingWorldPos = worldPos + new Vector3(facingDirection.x, 0, -facingDirection.y) * 0.5f;
            Gizmos.DrawLine(worldPos, facingWorldPos);
            Gizmos.DrawWireSphere(facingWorldPos, 0.1f);
            
            // Draw vision range (if there's a valid connection)
            var graph = _levelManager.GetCurrentGraph();
            var visionNode = _currentNodeId + facingDirection;
            
            if (graph.CanMoveFromTo(_currentNodeId, visionNode))
            {
                var visionWorldPos = _levelManager.GetNodeActualWorldPosition(visionNode);
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Semi-transparent red
                Gizmos.DrawCube(visionWorldPos, Vector3.one * 0.6f);
                
                // Draw vision line
                Gizmos.color = Color.red;
                Gizmos.DrawLine(worldPos, visionWorldPos);
            }
        }
    }
}