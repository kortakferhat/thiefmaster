using UnityEngine;
using Gameplay.Graph;
using Gameplay.Events;
using Infrastructure.Managers.LevelManager;
using Infrastructure;
using DG.Tweening;
using Infrastructure.Managers;
using TowerClicker.Infrastructure;

namespace Gameplay.Enemy
{
    /// <summary>
    /// Grid-based stationary guard enemy for the graph puzzle game
    /// </summary>
    public class GridEnemy : BaseEntity, IPoolable
    {
        public enum EnemyState
        {
            Stationary,      // Sabit duran, sadece vision detection
            Patrol,          // Git-gel yapan
            MovingTarget     // Player'ı takip eden
        }
        
        [Header("Enemy Settings")]
        [SerializeField] private Transform enemyTransform;
        [SerializeField] private Vector2Int facingDirection = Vector2Int.up;
        [SerializeField] private EnemyState currentState = EnemyState.Stationary;
        
        [Header("Movement Animation")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private float moveDelay = 0.5f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private Vector2Int _currentNodeId;
        private ITurnManager _turnManager;
        private ILevelManager _levelManager;
        private bool _isInitialized = false;
        private Vector2Int _currentPlayerNodeId; // Track player position
        private bool _isMoving = false;
        private Tween _currentMoveTween;
        
        public Vector2Int CurrentNodeId => _currentNodeId;
        public Vector2Int FacingDirection => facingDirection;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _turnManager = ServiceLocator.Get<ITurnManager>();
            _levelManager = ServiceLocator.Get<ILevelManager>();
            
            // Subscribe to events
            EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            
            // Get start node position
            var startNode = _levelManager.GetCurrentGraph().GetStartNode();
            _currentPlayerNodeId = startNode.Id;
            
            _isInitialized = true;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Initialized at {_currentNodeId}, facing {facingDirection}, state: {currentState}");
        }
        
        /// <summary>
        /// Set the enemy's position on the grid
        /// </summary>
        public void SetPosition(Vector2Int nodeId)
        {
            _currentNodeId = nodeId;
            
            // Update visual position
            var worldPos = _levelManager.GetNodeActualWorldPosition(nodeId);
            enemyTransform.position = worldPos;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Position set to {nodeId} (world: {worldPos})");
        }
        
        /// <summary>
        /// Move enemy to a new node with animation
        /// </summary>
        public void MoveToNode(Vector2Int newNodeId)
        {
            if (_isMoving) 
            {
                if (showDebugLogs)
                    Debug.Log($"[GridEnemy] Already moving, ignoring move request to {newNodeId}");
                return;
            }

            var previousNodeId = _currentNodeId;
            
            // Kill previous tween if it exists
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill();
            }

            // Calculate movement direction for rotation
            var moveDirection = newNodeId - previousNodeId;
            
            // Get target world position
            var targetPos = _levelManager.GetNodeActualWorldPosition(newNodeId);
            var targetRotation = GetRotationFromDirection(moveDirection);
            
            _isMoving = true;
            
            // Create delay tween first, then animate movement
            _currentMoveTween = DOVirtual.DelayedCall(moveDelay, () => {
                // Check if still moving (not interrupted)
                if (!_isMoving) return;
                
                // Animate movement and rotation
                DOTween.Sequence()
                    .Insert(0f, enemyTransform.DORotate(targetRotation, moveDuration * 0.5f).SetEase(moveEase))
                    .Insert(0f, enemyTransform.DOMove(targetPos, moveDuration).SetEase(moveEase))
                    .OnComplete(() => OnMoveComplete(newNodeId))
                    .SetAutoKill(true);
            }).SetAutoKill(true);
        }
        
        /// <summary>
        /// Animate movement to target node with delay
        /// </summary>
        
        
        /// <summary>
        /// Called when movement animation completes
        /// </summary>
        private void OnMoveComplete(Vector2Int newNodeId)
        {
            _isMoving = false;
            _currentMoveTween = null; // Clear tween reference
            _currentNodeId = newNodeId; // Update current node ID after animation
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Movement completed to {newNodeId}");
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
        
        /// <summary>
        /// Set the enemy's behavior state
        /// </summary>
        public void SetEnemyState(EnemyState newState)
        {
            var previousState = currentState;
            currentState = newState;
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] State changed from {previousState} to {newState}");
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
                EventBus.Publish(new GameOverEvent(_turnManager.CurrentTurn));
                return;
            }
        }
        
        /// <summary>
        /// Check if player is in vision range (1 edge distance in facing direction)
        /// </summary>
        private bool IsPlayerInVision(Vector2Int playerPosition)
        {
            var graph = _levelManager.GetCurrentGraph();
            
            // Calculate the node we're looking at (1 step in facing direction)
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
        /// Enemy moves based on current state
        /// </summary>
        private void PerformEnemyMovement()
        {
            switch (currentState)
            {
                case EnemyState.Stationary:
                    // Stationary enemies don't move
                    if (showDebugLogs)
                        Debug.Log($"[GridEnemy] Stationary - no movement");
                    break;
                    
                case EnemyState.Patrol:
                    PerformPatrolMovement();
                    break;
                    
                case EnemyState.MovingTarget:
                    PerformMovingTargetMovement();
                    break;
            }
        }
        
        /// <summary>
        /// Patrol movement: move in facing direction, reverse if blocked
        /// </summary>
        private void PerformPatrolMovement()
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
        /// MovingTarget movement: follow player if within 1 edge distance
        /// </summary>
        private void PerformMovingTargetMovement()
        {
            var graph = _levelManager.GetCurrentGraph();
            
            // Check if player is within 1 edge distance
            if (IsPlayerInVision(_currentPlayerNodeId))
            {
                // Player is visible - calculate direction to player
                var directionToPlayer = _currentPlayerNodeId - _currentNodeId;
                var targetNode = _currentNodeId + directionToPlayer;
                
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
                    
                    // Move towards player
                    MoveToNode(targetNode);
                }
            }
            else
            {
                // Player is too far (more than 1 edge) - switch to stationary
                SetEnemyState(EnemyState.Stationary);
                if (showDebugLogs)
                    Debug.Log($"[GridEnemy] Player too far, switching to Stationary");
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
            // Kill any active tweens
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill();
                _currentMoveTween = null;
            }
            
            _isMoving = false;
            
            // Unsubscribe from events
            EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        }
        
        #region IPoolable Implementation
        
        public void OnSpawnFromPool()
        {
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Spawned from pool at {_currentNodeId}");
            
            // Reset tween state
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill();
                _currentMoveTween = null;
            }
            
            _isMoving = false;
            _currentPlayerNodeId = Vector2Int.zero;
            
            // Initialize if not already done
            if (!_isInitialized)
            {
                Initialize();
            }
        }
        
        public void OnDespawn()
        {
            if (showDebugLogs)
                Debug.Log($"[GridEnemy] Despawned from pool");
            
            // Kill any active tweens
            if (_currentMoveTween != null && _currentMoveTween.IsActive())
            {
                _currentMoveTween.Kill();
                _currentMoveTween = null;
            }
            
            _isMoving = false;
            _currentPlayerNodeId = Vector2Int.zero;
            
            // Unsubscribe from events
            EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            
            _isInitialized = false;
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmos()
        {
            if (!_isInitialized) return;
            
            var worldPos = _levelManager?.GetNodeActualWorldPosition(_currentNodeId) ?? Vector3.zero;
            
            // Draw enemy position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(worldPos, 0.3f);
            
            // Draw facing direction
            Gizmos.color = Color.blue;
            var directionEnd = worldPos + new Vector3(facingDirection.x, 0, facingDirection.y) * 0.5f;
            Gizmos.DrawLine(worldPos, directionEnd);
            Gizmos.DrawWireSphere(directionEnd, 0.1f);
            
            // Draw state indicator
            Gizmos.color = GetStateColor(currentState);
            var statePos = worldPos + Vector3.up * 0.8f;
            Gizmos.DrawWireCube(statePos, Vector3.one * 0.2f);
        }
        
        private Color GetStateColor(EnemyState state)
        {
            return state switch
            {
                EnemyState.Stationary => Color.green,
                EnemyState.Patrol => Color.blue,
                EnemyState.MovingTarget => Color.magenta,
                _ => Color.white
            };
        }
        
        #endregion
    }
}