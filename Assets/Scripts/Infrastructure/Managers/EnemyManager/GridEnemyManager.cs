using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gameplay.Enemy;
using Gameplay.Enemy.Behaviours;
using Gameplay.Graph;
using Infrastructure.Managers.LevelManager;
using Infrastructure.Managers.PoolManager;

namespace TowerClicker.Infrastructure
{
    /// <summary>
    /// Manager for grid-based enemies in the puzzle game
    /// </summary>
    public class GridEnemyManager : MonoBehaviour, IGridEnemyManager
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private ILevelManager _levelManager;
        private IGameManager _gameManager;
        private IPoolManager _poolManager;
        private bool _initialized = false;
        
        private readonly List<GridEnemy> _activeEnemies = new();
        
        public IReadOnlyList<GridEnemy> ActiveEnemies => _activeEnemies;
        
        public void Initialize(ILevelManager levelManager, IGameManager gameManager)
        {
            _levelManager = levelManager;
            _gameManager = gameManager;
            _poolManager = ServiceLocator.Get<IPoolManager>();
            
            // Subscribe to level events to spawn enemies when level loads
            _levelManager.OnGridInstantiated += OnGridInstantiated;
            
            _initialized = true;
            
            if (showDebugLogs)
                Debug.Log("[GridEnemyManager] Initialized");
        }
        
        private void OnGridInstantiated(Graph graph)
        {
            SpawnEnemiesFromGraph(graph);
        }
        
        /// <summary>
        /// Handle level restart by restoring original enemy nodes and respawning enemies
        /// </summary>
        public void OnLevelRestart()
        {
            if (showDebugLogs)
                Debug.Log("[GridEnemyManager] Handling level restart");
            
            // Clear existing enemies first
            ClearAllEnemies();
            
            // Get the current level graph
            var currentLevel = _levelManager.CurrentLevelGraph;
            if (currentLevel == null || currentLevel.graphData == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[GridEnemyManager] No level data available for restart");
                return;
            }
            
            // Restore original enemy nodes from the level data
            var originalEnemyNodes = currentLevel.graphData.nodes
                .Where(n => n.type == NodeType.Enemy)
                .ToList();
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemyManager] Found {originalEnemyNodes.Count} original enemy nodes for restart");
            
            // Spawn enemies at their original positions
            foreach (var enemyNodeData in originalEnemyNodes)
            {
                var facingDirection = enemyNodeData.enemyFacingDirection;
                if (facingDirection == Vector2Int.zero)
                    facingDirection = Vector2Int.up;
                
                SpawnEnemyAtNode(enemyNodeData.id, facingDirection);
                
                // Note: We don't modify the original level data here
                // The graph will be handled by the normal OnGridInstantiated flow
            }
        }
        
        private void SpawnEnemiesFromGraph(Graph graph)
        {
            // Clear existing enemies
            ClearAllEnemies();
            
            // Find all enemy nodes in the graph
            var enemyNodes = FindEnemyNodes(graph);
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemyManager] Found {enemyNodes.Count} enemy spawn points");
            
            // Spawn enemy for each enemy node
            foreach (var enemyNode in enemyNodes)
            {
                // Get facing direction from NodeData if available
                var nodeData = GetNodeData(enemyNode.Id);
                var facingDirection = nodeData?.enemyFacingDirection ?? Vector2Int.up;
                
                SpawnEnemyAtNode(enemyNode.Id, facingDirection);
                
                // Convert enemy spawn node to normal node since enemy now occupies it
                // Only do this if we're not in a restart scenario
                graph.SetNodeType(enemyNode.Id, NodeType.Normal);
            }
        }
        
        private List<Node> FindEnemyNodes(Graph graph)
        {
            // Get all enemy nodes from the graph
            return graph.GetNodesByType(NodeType.Enemy).ToList();
        }
        
        private NodeData GetNodeData(Vector2Int nodeId)
        {
            // Get current level's graph data to access NodeData with enemy properties
            var currentLevel = _levelManager.CurrentLevelGraph;
            return currentLevel?.graphData?.nodes?.FirstOrDefault(n => n.id == nodeId);
        }
        
        public void SpawnEnemyAtNode(Vector2Int nodeId, Vector2Int facingDirection = default)
        {
            
            // Use default facing direction if none provided
            if (facingDirection == Vector2Int.zero)
                facingDirection = Vector2Int.up;
            
            // Get world position for the enemy
            var worldPos = _levelManager.GetNodeActualWorldPosition(nodeId);
            
            // Spawn enemy from pool
            var enemyGO = _poolManager.Spawn(PoolKeys.Enemy, transform, worldPos, Quaternion.identity);
            
            // Name the enemy
            enemyGO.name = $"GridEnemy_{nodeId.x}_{nodeId.y}";
            
            // Get or add GridEnemy component
            var gridEnemy = enemyGO.GetComponent<GridEnemy>() ?? enemyGO.AddComponent<GridEnemy>();
            
            // Initialize enemy
            gridEnemy.Initialize();
            gridEnemy.SetPosition(nodeId);
            gridEnemy.SetFacingDirection(facingDirection);
            
            // Set enemy behaviour from NodeData if available
            var nodeData = GetNodeData(nodeId);
            if (nodeData != null)
            {
                var behaviour = CreateBehaviourFromType(nodeData.enemyBehaviourType);
                if (behaviour != null)
                {
                    gridEnemy.SetBehaviour(behaviour);
                }
            }
            
            // Add to active enemies list
            _activeEnemies.Add(gridEnemy);
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemyManager] Spawned enemy from pool at {nodeId} facing {facingDirection}, behaviour: {nodeData?.enemyBehaviourType ?? "Stationary"}");
        }
        
        public void RemoveEnemy(GridEnemy enemy)
        {
            _activeEnemies.Remove(enemy);
            
            // Return enemy to pool
            _poolManager.Despawn(PoolKeys.Enemy, enemy.gameObject);
            
            if (showDebugLogs)
                Debug.Log($"[GridEnemyManager] Removed enemy at {enemy.CurrentNodeId}");
        }
        
        public void ClearAllEnemies()
        {
            foreach (var enemy in _activeEnemies.ToList())
            {
                RemoveEnemy(enemy);
            }
            
            _activeEnemies.Clear();
            
            if (showDebugLogs)
                Debug.Log("[GridEnemyManager] Cleared all enemies");
        }
        
        public GridEnemy GetEnemyAtNode(Vector2Int nodeId)
        {
            return _activeEnemies.FirstOrDefault(enemy => enemy.CurrentNodeId == nodeId);
        }
        
        /// <summary>
        /// Check if a node is occupied by an enemy (runtime check, not node type)
        /// </summary>
        public bool IsNodeOccupiedByEnemy(Vector2Int nodeId)
        {
            return GetEnemyAtNode(nodeId) != null;
        }
        
        /// <summary>
        /// Get all enemy positions (for collision detection, pathfinding, etc.)
        /// </summary>
        public IEnumerable<Vector2Int> GetAllEnemyPositions()
        {
            return _activeEnemies.Select(enemy => enemy.CurrentNodeId);
        }
        
        private void OnDestroy()
        {
            _levelManager.OnGridInstantiated -= OnGridInstantiated;
            ClearAllEnemies();
        }
        
        /// <summary>
        /// Create enemy behaviour instance from string type
        /// </summary>
        private IEnemyBehaviour CreateBehaviourFromType(string behaviourType)
        {
            return behaviourType switch
            {
                "Stationary" => new StationaryEnemyBehaviour(),
                "Patrol" => new PatrolEnemyBehaviour(),
                "MovingTarget" => new MovingTargetEnemyBehaviour(),
                _ => new StationaryEnemyBehaviour() // Default fallback
            };
        }
        
        // Debug method to manually spawn test enemies
        [ContextMenu("Spawn Test Enemies")]
        private void SpawnTestEnemies()
        {
            // Spawn a few test enemies from pool
            SpawnEnemyAtNode(new Vector2Int(1, 1), Vector2Int.right);
            SpawnEnemyAtNode(new Vector2Int(-1, 0), Vector2Int.left);
            SpawnEnemyAtNode(new Vector2Int(0, -1), Vector2Int.up);
            
            Debug.Log("[GridEnemyManager] Spawned test enemies from pool");
        }
        
        [ContextMenu("Clear Test Enemies")]
        private void ClearTestEnemies()
        {
            ClearAllEnemies();
            Debug.Log("[GridEnemyManager] Cleared all test enemies");
        }
    }
}