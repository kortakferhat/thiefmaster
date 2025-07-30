using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gameplay.Graph;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Managers.LevelManager
{
    public class LevelManager : ILevelManager
    {
        private int _currentLevel = -1;
        private GraphScriptableObject _currentLevelGraph;
        private GridConfig _gridConfig;
        private Transform _gridParent;
        private IPoolManager _poolManager;
        
        private readonly List<GameObject> _spawnedNodes = new();
        private readonly List<GameObject> _spawnedEdges = new();
        
        public int CurrentLevel => _currentLevel;
        public GraphScriptableObject CurrentLevelGraph => _currentLevelGraph;
        
        public event Action<int> OnLevelChanged;
        public event Action<GraphScriptableObject> OnLevelLoaded;
        public event Action OnLevelCompleted;
        public event Action OnLevelFailed;

        public async Task Initialize()
        {
            try
            {
                // Load GridConfig from Addressables
                _gridConfig = await Addressables.LoadAssetAsync<GridConfig>("GridConfig").ToUniTask();
                
                if (_gridConfig != null)
                {
                    Debug.Log("GridConfig loaded successfully from Addressables");
                }
                else
                {
                    Debug.LogWarning("GridConfig not found in Addressables, creating default config");
                    _gridConfig = ScriptableObject.CreateInstance<GridConfig>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading GridConfig from Addressables: {ex.Message}");
                Debug.Log("Creating default GridConfig as fallback");
                _gridConfig = ScriptableObject.CreateInstance<GridConfig>();
            }
        }

        public async void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0)
            {
                Debug.LogError($"Level index {levelIndex} cannot be negative.");
                return;
            }
            
            string levelKey = $"level_{levelIndex}";
            
            try
            {
                var levelGraph = await Addressables.LoadAssetAsync<GraphScriptableObject>(levelKey).ToUniTask();
                
                if (levelGraph != null)
                {
                    _currentLevel = levelIndex;
                    LoadLevel(levelGraph);
                }
                else
                {
                    Debug.LogError($"Failed to load level {levelIndex} with key '{levelKey}'");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading level {levelIndex}: {ex.Message}");
            }
        }

        public void LoadLevel(GraphScriptableObject levelGraph)
        {
            if (levelGraph == null)
            {
                Debug.LogError("Cannot load null level graph");
                return;
            }
            
            _currentLevelGraph = levelGraph;
            
            // Generate the grid
            GenerateGrid();
            
            // Trigger events
            OnLevelChanged?.Invoke(_currentLevel);
            OnLevelLoaded?.Invoke(_currentLevelGraph);
            
            Debug.Log($"Level {_currentLevel} ('{levelGraph.graphName}') loaded and grid generated");
        }

        public void CompleteLevel()
        {
            OnLevelCompleted?.Invoke();
            Debug.Log($"Level {_currentLevel} completed!");
        }

        public void FailLevel()
        {
            OnLevelFailed?.Invoke();
            Debug.Log($"Level {_currentLevel} failed!");
        }

        public void RestartLevel()
        {
            if (_currentLevelGraph != null)
            {
                OnLevelLoaded?.Invoke(_currentLevelGraph);
                Debug.Log($"Level {_currentLevel} restarted");
            }
            else
            {
                Debug.LogWarning("No current level to restart");
            }
        }

        public void NextLevel()
        {
            int nextLevelIndex = _currentLevel + 1;
            Debug.Log($"Attempting to load next level: {nextLevelIndex}");
            LoadLevel(nextLevelIndex);
        }

        private void InitializeGridSystem()
        {
            if (_poolManager == null)
                _poolManager = ServiceLocator.Get<IPoolManager>();
                
            if (_gridParent == null)
            {
                var gridObject = new GameObject("LevelGrid");
                _gridParent = gridObject.transform;
            }
        }

        private void GenerateGrid()
        {
            InitializeGridSystem();
            ClearGrid();
            
            if (_currentLevelGraph == null) return;
            if (_gridConfig == null)
            {
                Debug.LogError("GridConfig is null! Make sure LevelManager.Initialize() was called.");
                return;
            }

            var graph = _currentLevelGraph.CreateGraph();
            
            // Generate nodes
            GenerateNodes(graph);
            
            // Generate edges
            GenerateEdges(graph);
            
            Debug.Log($"Grid generated with {_spawnedNodes.Count} nodes and {_spawnedEdges.Count} edges");
        }

        private void GenerateNodes(Graph graph)
        {
            foreach (var nodeData in _currentLevelGraph.graphData.nodes)
            {
                var worldPos = GetNodeWorldPosition(nodeData.id);
                var poolKey = GetNodePoolKey(nodeData.type);
                var nodeObj = _poolManager.Spawn(poolKey, _gridParent, worldPos, Quaternion.identity);
                
                if (nodeObj != null)
                {
                    _spawnedNodes.Add(nodeObj);
                }
            }
        }

        private void GenerateEdges(Graph graph)
        {
            foreach (var edgeData in _currentLevelGraph.graphData.edges)
            {
                var fromPos = GetNodeWorldPosition(edgeData.fromId);
                var toPos = GetNodeWorldPosition(edgeData.toId);
                var centerPos = (fromPos + toPos) / 2f;
                centerPos.y += _gridConfig.edgeYOffset;

                var direction = (toPos - fromPos).normalized;
                var rotation = Quaternion.LookRotation(direction);

                var poolKey = GetEdgePoolKey(edgeData.type);
                var edgeObj = _poolManager.Spawn(poolKey, _gridParent, centerPos, rotation);
                
                if (edgeObj != null)
                {
                    // Scale edge to match distance
                    var distance = Vector3.Distance(fromPos, toPos);
                    edgeObj.transform.localScale = new Vector3(_gridConfig.edgeWidth, _gridConfig.edgeWidth, distance);
                    _spawnedEdges.Add(edgeObj);
                }
            }
        }

        private Vector3 GetNodeWorldPosition(Vector2Int nodeId)
        {
            return new Vector3(
                nodeId.x * _gridConfig.gridSpacing,
                _gridConfig.nodeYPosition,
                -nodeId.y * _gridConfig.gridSpacing
            );
        }

        private void ClearGrid()
        {
            // Despawn all nodes - we don't track their types, so try all possible node types
            foreach (var node in _spawnedNodes)
            {
                if (node != null)
                {
                    // Try to despawn with different node types until successful
                    bool despawned = _poolManager.Despawn(PoolKeys.BaseNode, node) ||
                                   _poolManager.Despawn(PoolKeys.StartNode, node) ||
                                   _poolManager.Despawn(PoolKeys.GoalNode, node) ||
                                   _poolManager.Despawn(PoolKeys.BreakableNode, node) ||
                                   _poolManager.Despawn(PoolKeys.RedirectorNode, node) ||
                                   _poolManager.Despawn(PoolKeys.TrapNode, node) ||
                                   _poolManager.Despawn(PoolKeys.EnemyNode, node);
                    
                    if (!despawned)
                        UnityEngine.Object.Destroy(node);
                }
            }
            _spawnedNodes.Clear();

            // Despawn all edges
            foreach (var edge in _spawnedEdges)
            {
                if (edge != null)
                {
                    bool despawned = _poolManager.Despawn(PoolKeys.BaseEdge, edge) ||
                                   _poolManager.Despawn(PoolKeys.DirectedEdge, edge) ||
                                   _poolManager.Despawn(PoolKeys.SlipperyEdge, edge) ||
                                   _poolManager.Despawn(PoolKeys.BreakableEdge, edge);
                    
                    if (!despawned)
                        UnityEngine.Object.Destroy(edge);
                }
            }
            _spawnedEdges.Clear();
        }

        private string GetNodePoolKey(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.Normal => PoolKeys.BaseNode,
                NodeType.Start => PoolKeys.StartNode,
                NodeType.Goal => PoolKeys.GoalNode,
                NodeType.Breakable => PoolKeys.BreakableNode,
                NodeType.Redirector => PoolKeys.RedirectorNode,
                NodeType.Trap => PoolKeys.TrapNode,
                NodeType.Enemy => PoolKeys.EnemyNode,
                _ => PoolKeys.BaseNode
            };
        }

        private string GetEdgePoolKey(EdgeType edgeType)
        {
            return edgeType switch
            {
                EdgeType.Standard => PoolKeys.BaseEdge,
                EdgeType.Directed => PoolKeys.DirectedEdge,
                EdgeType.Slippery => PoolKeys.SlipperyEdge,
                EdgeType.Breakable => PoolKeys.BreakableEdge,
                _ => PoolKeys.BaseEdge
            };
        }

        public void SetGridConfig(GridConfig config)
        {
            Debug.LogWarning("SetGridConfig is deprecated. GridConfig is now loaded from Addressables during Initialize().");
            if (config != null)
            {
                Debug.Log("Overriding GridConfig with provided config for testing purposes.");
                _gridConfig = config;
            }
        }

        public void ResetLevelProgress()
        {
            _currentLevel = -1;
            ClearGrid();
            _currentLevelGraph = null;
            Debug.Log("Level progress reset");
        }


    }
} 