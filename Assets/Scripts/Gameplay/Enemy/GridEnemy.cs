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
        
        public Vector2Int CurrentNodeId => _currentNodeId;
        public Vector2Int FacingDirection => facingDirection;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _turnManager = ServiceLocator.Get<ITurnManager>();
            _levelManager = ServiceLocator.Get<ILevelManager>();
            
            // Subscribe to turn events
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            
            _isInitialized = true;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Initialized at node {_currentNodeId}, facing {facingDirection}");
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
            
            // Check if player is in vision range (we'll implement this next)
            CheckPlayerDetection(moveEvent.ToNodeId);
        }
        
        private void CheckPlayerDetection(Vector2Int playerNodeId)
        {
            // For now, just log if player is on the same node
            if (playerNodeId == _currentNodeId)
            {
                Debug.Log($"[GridEnemy] PLAYER DETECTED! Player entered enemy node {_currentNodeId} - Game Over!");
                // TODO: Trigger game over event
            }
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
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Spawned from pool");
        }
        
        public void OnDespawn()
        {
            // Clean up when returning to pool
            _isInitialized = false;
            
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
        }
    }
}