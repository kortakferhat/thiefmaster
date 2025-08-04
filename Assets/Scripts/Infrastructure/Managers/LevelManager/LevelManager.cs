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
        private Transform _gridRoot; // Root object for rotation
        private Transform _gridParent;
        private Transform _nodesParent;
        private Transform _edgesParent;
        private GameObject _levelObject; // Level object reference
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
                
                // Load default level
                LoadLevel(1);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading GridConfig from Addressables: {ex.Message}");
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
                _currentLevel = levelIndex;
                LoadLevel(levelGraph);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading level {levelIndex}: {ex.Message}");
            }
        }

        public void LoadLevel(GraphScriptableObject levelGraph)
        {
            _currentLevelGraph = levelGraph;
            
            // Generate the grid
            GenerateGrid();
            
            // Trigger events
            OnLevelChanged?.Invoke(_currentLevel);
            OnLevelLoaded?.Invoke(_currentLevelGraph);
        }

        public void CompleteLevel()
        {
            OnLevelCompleted?.Invoke();
        }

        public void FailLevel()
        {
            OnLevelFailed?.Invoke();
        }

        public void RestartLevel()
        {
            OnLevelLoaded?.Invoke(_currentLevelGraph);
        }

        public void NextLevel()
        {
            LoadLevel(_currentLevel + 1);
        }

        private void ApplyVerticalOffsetToLevel(GameObject levelObject)
        {
            float screenHeight = Camera.main.orthographicSize * 2f;
            float offset = (screenHeight * _gridConfig.verticalOffsetPercentage) + _gridConfig.additionalVerticalOffset;
            
            // Apply offset based on rotation for 3D perspective camera
            switch (_gridConfig.levelRotation)
            {
                case LevelRotation.Right: // 0°
                    levelObject.transform.localPosition = new Vector3(0, 0, -offset);
                    break;
                case LevelRotation.Down: // 90°
                    levelObject.transform.localPosition = new Vector3(-offset, 0, 0);
                    break;
                case LevelRotation.Left: // 180°
                    levelObject.transform.localPosition = new Vector3(0, 0, offset);
                    break;
                case LevelRotation.Up: // 270°
                    levelObject.transform.localPosition = new Vector3(offset, 0, 0);
                    break;
            }
        }

        private void InitializeGridSystem()
        {
            _poolManager = ServiceLocator.Get<IPoolManager>();
            
            // Create root object for rotation
            var rootObject = new GameObject("GridRoot");
            _gridRoot = rootObject.transform;
            
            // Create Level object as child of GridRoot
            _levelObject = new GameObject("Level");
            _levelObject.transform.SetParent(_gridRoot);
            
            // Create grid parent
            var gridObject = new GameObject("LevelGrid");
            _gridParent = gridObject.transform;
            _gridParent.SetParent(_levelObject.transform);
            
            // Create nodes parent
            var nodesObject = new GameObject("Nodes");
            nodesObject.transform.SetParent(_gridParent);
            _nodesParent = nodesObject.transform;
            
            // Create edges parent
            var edgesObject = new GameObject("Edges");
            edgesObject.transform.SetParent(_gridParent);
            _edgesParent = edgesObject.transform;
        }

        private void GenerateGrid()
        {
            InitializeGridSystem();
            ClearGrid();

            var graph = _currentLevelGraph.CreateGraph();
            
            // Calculate bounding box center as pivot point for node positioning
            Vector3 pivotPoint = CalculateBoundingBoxCenter();
            
            // LevelGrid stays at (0,0,0) - no offset needed
            _gridParent.localPosition = Vector3.zero;
            
            // Generate nodes
            GenerateNodes(graph);
            
            // Generate edges
            GenerateEdges(graph);
            
            // Apply rotation to the root object
            ApplyGridRotation();
            
            // Apply vertical offset to Level object after all content is generated
            ApplyVerticalOffsetToLevel(_levelObject);
        }

        private Vector3 CalculateBoundingBoxCenter()
        {
            var nodes = _currentLevelGraph.graphData.nodes;
            
            if (nodes.Count == 0)
                return Vector3.zero;
            
            // Find bounding box
            int minX = nodes[0].id.x, maxX = nodes[0].id.x;
            int minY = nodes[0].id.y, maxY = nodes[0].id.y;
            
            foreach (var node in nodes)
            {
                if (node.id.x < minX) minX = node.id.x;
                if (node.id.x > maxX) maxX = node.id.x;
                if (node.id.y < minY) minY = node.id.y;
                if (node.id.y > maxY) maxY = node.id.y;
            }
            
            // Calculate center in grid coordinates
            Vector2Int gridCenter = new Vector2Int((minX + maxX) / 2, (minY + maxY) / 2);
            
            // Convert to world position
            Vector3 worldCenter = new Vector3(
                gridCenter.x * _gridConfig.gridSpacing,
                _gridConfig.nodeYPosition,
                -gridCenter.y * _gridConfig.gridSpacing
            );
            
            return worldCenter;
        }

        private void GenerateNodes(Graph graph)
        {
            foreach (var nodeData in _currentLevelGraph.graphData.nodes)
            {
                var worldPos = GetNodeWorldPosition(nodeData.id);
                var poolKey = GetNodePoolKey(nodeData.type);
                var nodeObj = _poolManager.Spawn(poolKey, _nodesParent, worldPos, Quaternion.identity);
                
                // Name the node based on its type and ID
                nodeObj.name = $"{nodeData.type}Node_({nodeData.id.x},{nodeData.id.y})";
                _spawnedNodes.Add(nodeObj);
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
                var edgeObj = _poolManager.Spawn(poolKey, _edgesParent, centerPos, rotation);
                
                // Name the edge based on its type and connection IDs
                edgeObj.name = $"{edgeData.type}Edge_({edgeData.fromId.x},{edgeData.fromId.y})_to_({edgeData.toId.x},{edgeData.toId.y})";
                
                // Scale edge to match distance
                var distance = Vector3.Distance(fromPos, toPos);
                edgeObj.transform.localScale = new Vector3(_gridConfig.edgeWidth, _gridConfig.edgeWidth, distance);
                _spawnedEdges.Add(edgeObj);
            }
        }

        private Vector3 GetNodeWorldPosition(Vector2Int nodeId)
        {
            // Calculate position relative to grid center (pivot point)
            Vector3 pivotPoint = CalculateBoundingBoxCenter();
            
            // Node position relative to pivot point
            Vector3 position = new Vector3(
                nodeId.x * _gridConfig.gridSpacing,
                _gridConfig.nodeYPosition,
                -nodeId.y * _gridConfig.gridSpacing
            ) - pivotPoint; // Subtract pivot to center the grid
            
            return position;
        }

        private void ApplyGridRotation()
        {
            _gridRoot.rotation = Quaternion.Euler(0, (float)_gridConfig.levelRotation, 0);
        }

        private void ClearGrid()
        {
            // Reset root rotation and grid parent position before clearing
            _gridRoot.rotation = Quaternion.identity;
            _gridParent.localPosition = Vector3.zero;
            
            // Despawn all nodes - we don't track their types, so try all possible node types
            foreach (var node in _spawnedNodes)
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
            _spawnedNodes.Clear();

            // Despawn all edges
            foreach (var edge in _spawnedEdges)
            {
                bool despawned = _poolManager.Despawn(PoolKeys.BaseEdge, edge) ||
                               _poolManager.Despawn(PoolKeys.DirectedEdge, edge) ||
                               _poolManager.Despawn(PoolKeys.SlipperyEdge, edge) ||
                               _poolManager.Despawn(PoolKeys.BreakableEdge, edge);
                
                if (!despawned)
                    UnityEngine.Object.Destroy(edge);
            }
            _spawnedEdges.Clear();
            
            // Don't destroy parent objects, just keep them for reuse
            // They will be reused in the next level generation
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
            _gridConfig = config;
        }

        public void SetLevelRotation(LevelRotation rotation)
        {
            if (_gridConfig.levelRotation != rotation)
            {
                _gridConfig.levelRotation = rotation;
                ApplyGridRotation();
            }
        }

        public LevelRotation GetCurrentRotation()
        {
            return _gridConfig.levelRotation;
        }

        public void ResetLevelProgress()
        {
            _currentLevel = -1;
            ClearGrid();
            _currentLevelGraph = null;
            
            // Reset all grid references
            UnityEngine.Object.Destroy(_gridRoot.gameObject);
            _gridRoot = null;
            _gridParent = null;
            _nodesParent = null;
            _edgesParent = null;
            _levelObject = null;
        }
    }
} 