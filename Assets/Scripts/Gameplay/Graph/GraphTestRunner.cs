using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Graph
{
    public class GraphTestRunner : MonoBehaviour
    {
        [Header("Test Configuration")]
        public GraphScriptableObject testGraph;
        public bool runTestsOnStart = true;
        public bool logTestResults = true;

        [Header("Test Results")]
        [SerializeField] private bool testsPassed;
        [SerializeField] private string testResults;

        private void Start()
        {
            if (runTestsOnStart && testGraph != null)
            {
                RunAllTests();
            }
        }

        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            if (testGraph == null)
            {
                Debug.LogError("No test graph assigned!");
                return;
            }

            var results = new List<string>();
            var passed = true;

            // Test 1: Graph Creation
            try
            {
                var graph = testGraph.CreateGraph();
                results.Add("✓ Graph creation successful");
            }
            catch (System.Exception e)
            {
                results.Add($"✗ Graph creation failed: {e.Message}");
                passed = false;
            }

            // Test 2: Node Access
            try
            {
                var graph = testGraph.CreateGraph();
                var nodeCount = testGraph.graphData.nodes.Count;
                var accessibleNodes = 0;

                foreach (var nodeData in testGraph.graphData.nodes)
                {
                    var node = graph.GetNode(nodeData.id);
                    if (node != null && node.Id == nodeData.id)
                    {
                        accessibleNodes++;
                    }
                }

                if (accessibleNodes == nodeCount)
                {
                    results.Add($"✓ Node access test passed ({accessibleNodes}/{nodeCount} nodes accessible)");
                }
                else
                {
                    results.Add($"✗ Node access test failed ({accessibleNodes}/{nodeCount} nodes accessible)");
                    passed = false;
                }
            }
            catch (System.Exception e)
            {
                results.Add($"✗ Node access test failed: {e.Message}");
                passed = false;
            }

            // Test 3: Edge Validation
            try
            {
                var graph = testGraph.CreateGraph();
                var edgeCount = testGraph.graphData.edges.Count;
                var validEdges = 0;

                foreach (var edgeData in testGraph.graphData.edges)
                {
                    var fromNode = graph.GetNode(edgeData.fromId);
                    var toNode = graph.GetNode(edgeData.toId);

                    if (fromNode != null && toNode != null)
                    {
                        var edge = graph.GetEdge(fromNode, edgeData.toId - edgeData.fromId);
                        if (edge != null)
                        {
                            validEdges++;
                        }
                    }
                }

                if (validEdges == edgeCount)
                {
                    results.Add($"✓ Edge validation test passed ({validEdges}/{edgeCount} edges valid)");
                }
                else
                {
                    results.Add($"✗ Edge validation test failed ({validEdges}/{edgeCount} edges valid)");
                    passed = false;
                }
            }
            catch (System.Exception e)
            {
                results.Add($"✗ Edge validation test failed: {e.Message}");
                passed = false;
            }

            // Test 4: Graph Connectivity
            try
            {
                var graph = testGraph.CreateGraph();
                var startNodes = testGraph.graphData.nodes.Where(n => n.type == NodeType.Start).ToList();
                var goalNodes = testGraph.graphData.nodes.Where(n => n.type == NodeType.Goal).ToList();

                if (startNodes.Count > 0 && goalNodes.Count > 0)
                {
                    results.Add($"✓ Graph has start ({startNodes.Count}) and goal ({goalNodes.Count}) nodes");
                }
                else
                {
                    results.Add("⚠ Graph missing start or goal nodes");
                }
            }
            catch (System.Exception e)
            {
                results.Add($"✗ Graph connectivity test failed: {e.Message}");
                passed = false;
            }

            // Test 5: Duplicate Detection
            try
            {
                var nodeIds = testGraph.graphData.nodes.Select(n => n.id).ToList();
                var edgePairs = testGraph.graphData.edges.Select(e => new { From = e.fromId, To = e.toId }).ToList();

                var duplicateNodes = nodeIds.GroupBy(x => x).Where(g => g.Count() > 1).Count();
                var duplicateEdges = edgePairs.GroupBy(x => new { x.From, x.To }).Where(g => g.Count() > 1).Count();

                if (duplicateNodes == 0)
                {
                    results.Add("✓ No duplicate nodes found");
                }
                else
                {
                    results.Add($"✗ Found {duplicateNodes} duplicate nodes");
                    passed = false;
                }

                if (duplicateEdges == 0)
                {
                    results.Add("✓ No duplicate edges found");
                }
                else
                {
                    results.Add($"✗ Found {duplicateEdges} duplicate edges");
                    passed = false;
                }
            }
            catch (System.Exception e)
            {
                results.Add($"✗ Duplicate detection test failed: {e.Message}");
                passed = false;
            }

            // Store results
            testsPassed = passed;
            testResults = string.Join("\n", results);

            if (logTestResults)
            {
                Debug.Log($"Graph Test Results for '{testGraph.graphName}':\n{testResults}");
                
                if (passed)
                {
                    Debug.Log("🎉 All tests passed!");
                }
                else
                {
                    Debug.LogWarning("⚠ Some tests failed. Check the results above.");
                }
            }
        }

        [ContextMenu("Print Graph Info")]
        public void PrintGraphInfo()
        {
            if (testGraph == null)
            {
                Debug.LogError("No test graph assigned!");
                return;
            }

            var graph = testGraph.CreateGraph();
            var nodeCount = testGraph.graphData.nodes.Count;
            var edgeCount = testGraph.graphData.edges.Count;

            Debug.Log($"Graph: {testGraph.graphName}");
            Debug.Log($"Description: {testGraph.description}");
            Debug.Log($"Nodes: {nodeCount}");
            Debug.Log($"Edges: {edgeCount}");

            // Node type breakdown
            var nodeTypes = testGraph.graphData.nodes.GroupBy(n => n.type)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();
            Debug.Log($"Node Types: {string.Join(", ", nodeTypes)}");

            // Edge type breakdown
            var edgeTypes = testGraph.graphData.edges.GroupBy(e => e.type)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();
            Debug.Log($"Edge Types: {string.Join(", ", edgeTypes)}");
        }

        [ContextMenu("Validate Graph Solvability")]
        public void ValidateGraphSolvability()
        {
            if (testGraph == null)
            {
                Debug.LogError("No test graph assigned!");
                return;
            }

            var graph = testGraph.CreateGraph();
            var startNodes = testGraph.graphData.nodes.Where(n => n.type == NodeType.Start).ToList();
            var goalNodes = testGraph.graphData.nodes.Where(n => n.type == NodeType.Goal).ToList();

            if (startNodes.Count == 0)
            {
                Debug.LogWarning("No start nodes found in graph!");
                return;
            }

            if (goalNodes.Count == 0)
            {
                Debug.LogWarning("No goal nodes found in graph!");
                return;
            }

            var solvablePaths = 0;
            var totalPaths = startNodes.Count * goalNodes.Count;

            foreach (var startNode in startNodes)
            {
                foreach (var goalNode in goalNodes)
                {
                    if (IsPathPossible(graph, startNode, goalNode))
                    {
                        solvablePaths++;
                    }
                }
            }

            var solvabilityPercentage = (float)solvablePaths / totalPaths * 100f;
            Debug.Log($"Graph Solvability: {solvablePaths}/{totalPaths} paths possible ({solvabilityPercentage:F1}%)");

            if (solvablePaths == 0)
            {
                Debug.LogWarning("⚠ Graph appears to be unsolvable!");
            }
            else if (solvablePaths < totalPaths)
            {
                Debug.LogWarning("⚠ Some start-goal combinations are not reachable");
            }
            else
            {
                Debug.Log("✓ All start-goal combinations are reachable");
            }
        }

        private bool IsPathPossible(Graph graph, NodeData start, NodeData goal)
        {
            // Simple BFS to check if path exists
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            
            queue.Enqueue(start.id);
            visited.Add(start.id);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                
                if (currentId == goal.id)
                {
                    return true;
                }

                var currentNode = graph.GetNode(currentId);
                if (currentNode == null) continue;

                // Check all 4 directions
                var directions = new Vector2Int[]
                {
                    Vector2Int.up,
                    Vector2Int.right,
                    Vector2Int.down,
                    Vector2Int.left
                };

                foreach (var direction in directions)
                {
                    var edge = graph.GetEdge(currentNode, direction);
                    if (edge != null && !edge.IsUsed && !edge.To.IsDestroyed)
                    {
                        var nextId = edge.To.Id;
                        if (!visited.Contains(nextId))
                        {
                            visited.Add(nextId);
                            queue.Enqueue(nextId);
                        }
                    }
                }
            }

            return false;
        }

        private void OnValidate()
        {
            if (testGraph != null && testGraph.graphData == null)
            {
                testGraph.graphData = new GraphData();
            }
        }
    }
} 