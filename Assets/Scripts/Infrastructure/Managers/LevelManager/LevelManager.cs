using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gameplay.Graph;
using Gameplay.Events;
using Infrastructure.Managers.PoolManager;
using Infrastructure;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Managers.LevelManager
{
    public class LevelManager : ILevelManager
    {
        // Core state
        private int _currentLevel = -1;
        private GraphScriptableObject _currentLevelGraph;
        private Graph _currentGraph;
        
        // Configuration
        private GridConfig _gridConfig;
        
        // Services - initialized once
        private IPoolManager _poolManager;
        private IGameManager _gameManager;
        
        // Grid hierarchy - single root object
        private GameObject _gridRoot;
        private Transform _nodesParent;
        private Transform _edgesParent;
        
        // Spawned objects
        private readonly List<GameObject> _spawnedNodes = new();
        private readonly List<GameObject> _spawnedEdges = new();
        private readonly Dictionary<Vector2Int, GameObject> _nodeObjects = new();
        
        // Events
        public event Action<int> OnLevelChanged;
        public event Action<GraphScriptableObject> OnLevelLoaded;
        public event Action OnLevelCompleted;
        public event Action OnLevelFailed;
        public event Action<Graph> OnGridGenerated;
        public event Action<Graph> OnGridInstantiated;
        
        // Properties
        public int CurrentLevel => _currentLevel;
        public GraphScriptableObject CurrentLevelGraph => _currentLevelGraph;
        public GridConfig GetGridConfig() => _gridConfig;
        public Graph GetCurrentGraph() => _currentGraph;

        public async Task Initialize()
        {
            try
            {
                // Load configuration
                _gridConfig = await Addressables.LoadAssetAsync<GridConfig>("GridConfig").ToUniTask();
                
                // Get services once
                _poolManager = ServiceLocator.Get<IPoolManager>();
                _gameManager = ServiceLocator.Get<IGameManager>();
                
                // Subscribe to events
                EventBus.Subscribe<GameEvents.GameStateChangeEvent>(OnGameStateChangeEvent);
                
                // Load first level
                LoadLevel(1);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LevelManager] Initialization failed: {ex.Message}");
                throw;
            }
        }
        
        public async void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0)
            {
                Debug.LogError($"[LevelManager] Invalid level index: {levelIndex}");
                return;
            }
            
            try
            {
                var levelGraph = await Addressables.LoadAssetAsync<GraphScriptableObject>($"level_{levelIndex}").ToUniTask();
                LoadLevelInternal(levelGraph, levelIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LevelManager] Failed to load level {levelIndex}: {ex.Message}");
            }
        }

        public void LoadLevel(GraphScriptableObject levelGraph)
        {
            LoadLevelInternal(levelGraph, _currentLevel);
        }

        private void LoadLevelInternal(GraphScriptableObject levelGraph, int levelIndex)
        {
            // Unload current level if exists (return objects to pool)
            if (_currentLevelGraph != null)
            {
                UnloadLevel();
            }
            
            // Clear grid hierarchy if exists
            if (_gridRoot != null)
            {
                UnityEngine.Object.Destroy(_gridRoot);
                _gridRoot = null;
                _nodesParent = null;
                _edgesParent = null;
            }
            
            // Set new level
            _currentLevel = levelIndex;
            _currentLevelGraph = levelGraph;
            
            // Generate new level
            GenerateLevel();
            
            // Notify
            OnLevelChanged?.Invoke(_currentLevel);
            OnLevelLoaded?.Invoke(_currentLevelGraph);
        }

        public void RestartLevel()
        {
            if (_currentLevelGraph == null)
            {
                Debug.LogWarning("[LevelManager] No level to restart");
                return;
            }
            
            Debug.Log($"[LevelManager] Restarting level {_currentLevel}");
            
            // First unload current level (return objects to pool)
            UnloadLevel();
            
            // Clear grid hierarchy
            if (_gridRoot != null)
            {
                UnityEngine.Object.Destroy(_gridRoot);
                _gridRoot = null;
                _nodesParent = null;
                _edgesParent = null;
            }
            
            // Wait one frame to ensure all objects are properly destroyed
            // This prevents the MissingReferenceException from destroyed GameObjects
            RestartLevelAsync().Forget();
        }
        
        private async UniTaskVoid RestartLevelAsync()
        {
            // Wait for next frame to ensure all objects are destroyed
            await UniTask.NextFrame();
            
            // Now regenerate the level
            GenerateLevel();
            
            // Reset game state
            _gameManager.StartGame();
            
            // Reset turn manager
            ServiceLocator.Get<ITurnManager>()?.ResetForNewLevel();
            
            // Notify restart
            OnLevelLoaded?.Invoke(_currentLevelGraph);
            
            // Handle enemies
            ServiceLocator.Get<IGridEnemyManager>()?.OnLevelRestart();
        }

        public void NextLevel()
        {
            if (_currentLevel < 0) return;
            LoadLevel(_currentLevel + 1);
        }

        public void CompleteLevel()
        {
            OnLevelCompleted?.Invoke();
        }

        public void FailLevel()
        {
            OnLevelFailed?.Invoke();
        }
        
        
        private void OnGameStateChangeEvent(GameEvents.GameStateChangeEvent args)
        {
            if (args.CurrentState is GameState.Finish)
            {
                if (args.Reason == GameEvents.GameEventChangeReason.Win)
                {
                    CompleteLevel();
                    _gameManager.WinGame();
                    return;
                }

                if (args.Reason == GameEvents.GameEventChangeReason.Lose)
                {
                    FailLevel();
                    return;
                }
            }
        }

        private void GenerateLevel()
        {
            if (_currentLevelGraph == null) return;
            
            // Create grid hierarchy
            CreateGridHierarchy();
            
            // Generate graph
            _currentGraph = _currentLevelGraph.CreateGraph();
            
            // Generate content
            GenerateNodes();
            GenerateEdges();
            
            // Apply positioning
            PositionGrid();
            
            // Notify
            OnGridGenerated?.Invoke(_currentGraph);
            OnGridInstantiated?.Invoke(_currentGraph);
        }

        private void CreateGridHierarchy()
        {
            // Single root object
            _gridRoot = new GameObject("Level");
            
            // Create parent transforms
            _nodesParent = new GameObject("Nodes").transform;
            _edgesParent = new GameObject("Edges").transform;
            
            // Setup hierarchy
            _nodesParent.SetParent(_gridRoot.transform);
            _edgesParent.SetParent(_gridRoot.transform);
            
            Debug.Log("[LevelManager] Grid hierarchy created successfully");
        }

        private void GenerateNodes()
        {
            if (_currentLevelGraph?.graphData?.nodes == null) return;
            
            foreach (var nodeData in _currentLevelGraph.graphData.nodes)
            {
                var worldPos = CalculateNodePosition(nodeData.id);
                var poolKey = GetNodePoolKey(nodeData.type);
                
                var nodeObj = _poolManager.Spawn(poolKey, _nodesParent, worldPos, Quaternion.identity);
                if (nodeObj == null)
                {
                    Debug.LogError($"[LevelManager] Failed to spawn node of type {nodeData.type} from pool {poolKey}");
                    continue;
                }
                
                nodeObj.name = $"{nodeData.type}Node_({nodeData.id.x},{nodeData.id.y})";
                
                // Add gizmo
                var nodeGizmo = nodeObj.AddComponent<Gameplay.Graph.NodeGizmo>();
                nodeGizmo.Initialize(nodeData.id, nodeData.type);
                
                _spawnedNodes.Add(nodeObj);
                _nodeObjects[nodeData.id] = nodeObj;
            }
        }

        private void GenerateEdges()
        {
            if (_currentLevelGraph?.graphData?.edges == null) return;
            
            foreach (var edgeData in _currentLevelGraph.graphData.edges)
            {
                var fromPos = CalculateNodePosition(edgeData.fromId);
                var toPos = CalculateNodePosition(edgeData.toId);
                var centerPos = (fromPos + toPos) * 0.5f;
                centerPos.y += _gridConfig.edgeYOffset;

                var direction = (toPos - fromPos).normalized;
                var rotation = Quaternion.LookRotation(direction);

                var poolKey = GetEdgePoolKey(edgeData.type);
                var edgeObj = _poolManager.Spawn(poolKey, _edgesParent, centerPos, rotation);
                
                if (edgeObj == null)
                {
                    Debug.LogError($"[LevelManager] Failed to spawn edge of type {edgeData.type} from pool {poolKey}");
                    continue;
                }
                
                edgeObj.name = $"{edgeData.type}Edge_({edgeData.fromId.x},{edgeData.fromId.y})_to_({edgeData.toId.x},{edgeData.toId.y})";
                
                // Scale edge
                var distance = Vector3.Distance(fromPos, toPos);
                edgeObj.transform.localScale = new Vector3(_gridConfig.edgeWidth, _gridConfig.edgeWidth, distance);
                
                _spawnedEdges.Add(edgeObj);
            }
        }

        private Vector3 CalculateNodePosition(Vector2Int nodeId)
        {
            var gridPos = new Vector3(
                nodeId.x * _gridConfig.gridSpacing,
                _gridConfig.nodeYPosition,
                nodeId.y * _gridConfig.gridSpacing
            );
            
            // Center the grid
            var gridCenter = CalculateGridCenter();
            return gridPos - gridCenter;
        }

        private Vector3 CalculateGridCenter()
        {
            if (_currentLevelGraph?.graphData?.nodes == null || _currentLevelGraph.graphData.nodes.Count == 0)
                return Vector3.zero;
            
            var nodes = _currentLevelGraph.graphData.nodes;
            
            // Find bounds
            int minX = nodes[0].id.x, maxX = nodes[0].id.x;
            int minY = nodes[0].id.y, maxY = nodes[0].id.y;
            
            foreach (var node in nodes)
            {
                minX = Mathf.Min(minX, node.id.x);
                maxX = Mathf.Max(maxX, node.id.x);
                minY = Mathf.Min(minY, node.id.y);
                maxY = Mathf.Max(maxY, node.id.y);
            }
            
            // Calculate center
            var gridCenter = new Vector2Int((minX + maxX) / 2, (minY + maxY) / 2);
            return new Vector3(
                gridCenter.x * _gridConfig.gridSpacing,
                _gridConfig.nodeYPosition,
                gridCenter.y * _gridConfig.gridSpacing
            );
        }

        private void PositionGrid()
        {
            if (_gridRoot == null) return;
            
            // Apply vertical offset
            var screenHeight = Camera.main.orthographicSize * 2f;
            var offset = (screenHeight * _gridConfig.verticalOffsetPercentage) + _gridConfig.additionalVerticalOffset;
            _gridRoot.transform.localPosition = new Vector3(0, 0, offset);
        }

        public void UnloadLevel()
        {
            Debug.Log("[LevelManager] Unloading current level");
            
            // Return all spawned objects to pool
            ReturnObjectsToPool();
            
            // Clear references
            _spawnedNodes.Clear();
            _spawnedEdges.Clear();
            _nodeObjects.Clear();
            
            // Clear graph reference
            _currentGraph = null;
        }
        
        private void ReturnObjectsToPool()
        {
            // Return nodes to pool
            foreach (var node in _spawnedNodes)
            {
                if (node != null)
                {
                    TryDespawnNode(node);
                }
            }
            
            // Return edges to pool
            foreach (var edge in _spawnedEdges)
            {
                if (edge != null)
                {
                    TryDespawnEdge(edge);
                }
            }
        }

        private void ClearLevel()
        {
            // First return objects to pool (proper cleanup)
            ReturnObjectsToPool();
            
            // Clear references
            _spawnedNodes.Clear();
            _spawnedEdges.Clear();
            _nodeObjects.Clear();
            
            // Destroy grid hierarchy
            if (_gridRoot != null)
            {
                UnityEngine.Object.Destroy(_gridRoot);
                _gridRoot = null;
                _nodesParent = null;
                _edgesParent = null;
            }
            
            // Clear references
            _currentGraph = null;
        }

        private void ClearSpawnedObjects()
        {
            // This method is now simplified since ReturnObjectsToPool handles the work
            // Just clear the lists
            _spawnedNodes.Clear();
            _spawnedEdges.Clear();
            _nodeObjects.Clear();
        }

        private void TryDespawnNode(GameObject node)
        {
            if (node == null) return;
            
            // Try to despawn with different types
            var despawned = _poolManager.Despawn(PoolKeys.BaseNode, node) ||
                           _poolManager.Despawn(PoolKeys.StartNode, node) ||
                           _poolManager.Despawn(PoolKeys.GoalNode, node) ||
                           _poolManager.Despawn(PoolKeys.BreakableNode, node) ||
                           _poolManager.Despawn(PoolKeys.RedirectorNode, node) ||
                           _poolManager.Despawn(PoolKeys.TrapNode, node) ||
                           _poolManager.Despawn(PoolKeys.Enemy, node);
            
            if (!despawned)
            {
                Debug.LogWarning($"[LevelManager] Failed to despawn node {node.name}, destroying instead");
                UnityEngine.Object.DestroyImmediate(node);
            }
        }

        private void TryDespawnEdge(GameObject edge)
        {
            if (edge == null) return;
            
            var despawned = _poolManager.Despawn(PoolKeys.BaseEdge, edge) ||
                           _poolManager.Despawn(PoolKeys.DirectedEdge, edge) ||
                           _poolManager.Despawn(PoolKeys.SlipperyEdge, edge) ||
                           _poolManager.Despawn(PoolKeys.BreakableEdge, edge);
            
            if (!despawned)
            {
                Debug.LogWarning($"[LevelManager] Failed to despawn edge {edge.name}, destroying instead");
                UnityEngine.Object.DestroyImmediate(edge);
            }
        }

        public Vector3 GetNodeWorldPosition(Vector2Int nodeId)
        {
            return CalculateNodePosition(nodeId);
        }

        public Vector3 GetNodeActualWorldPosition(Vector2Int nodeId)
        {
            return _nodeObjects.TryGetValue(nodeId, out var nodeObj) 
                ? nodeObj.transform.position 
                : CalculateNodePosition(nodeId);
        }

        public bool TryMoveToNode(Vector2Int currentNodeId, Vector2Int direction, out Vector2Int targetNodeId)
        {
            targetNodeId = currentNodeId + direction;
            return _currentGraph?.CanMoveFromTo(currentNodeId, targetNodeId) ?? false;
        }

        public void SetGridConfig(GridConfig config)
        {
            _gridConfig = config;
        }

        public void ResetLevelProgress()
        {
            _currentLevel = -1;
            
            // Unload current level if exists
            if (_currentLevelGraph != null)
            {
                UnloadLevel();
            }
            
            // Clear grid hierarchy if exists
            if (_gridRoot != null)
            {
                UnityEngine.Object.Destroy(_gridRoot);
                _gridRoot = null;
                _nodesParent = null;
                _edgesParent = null;
            }
            
            _currentLevelGraph = null;
        }

        private void OnDestroy()
        {
        }

        private string GetNodePoolKey(NodeType nodeType) => nodeType switch
        {
            NodeType.Normal => PoolKeys.BaseNode,
            NodeType.Start => PoolKeys.StartNode,
            NodeType.Goal => PoolKeys.GoalNode,
            NodeType.Breakable => PoolKeys.BreakableNode,
            NodeType.Redirector => PoolKeys.RedirectorNode,
            NodeType.Trap => PoolKeys.TrapNode,
            NodeType.Enemy => PoolKeys.BaseNode,
            _ => PoolKeys.BaseNode
        };

        private string GetEdgePoolKey(EdgeType edgeType) => edgeType switch
        {
            EdgeType.Standard => PoolKeys.BaseEdge,
            EdgeType.Directed => PoolKeys.DirectedEdge,
            EdgeType.Slippery => PoolKeys.SlipperyEdge,
            EdgeType.Breakable => PoolKeys.BreakableEdge,
            _ => PoolKeys.BaseEdge
        };
    }
} 