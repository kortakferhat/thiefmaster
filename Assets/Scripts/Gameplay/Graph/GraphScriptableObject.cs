using UnityEngine;

namespace Gameplay.Graph
{
    [CreateAssetMenu(fileName = "New Graph", menuName = "ThiefMaster/Graph Data")]
    public class GraphScriptableObject : ScriptableObject
    {
        [Header("Graph Information")]
        public string graphName = "New Graph";
        public string description = "";
        
        [Header("Graph Data")]
        public GraphData graphData = new();

        [Header("Editor Settings")]
        public float nodeSize = 30f;
        public float edgeWidth = 6f;

        private void OnValidate()
        {
            if (graphData != null)
            {
                graphData.graphName = graphName;
            }
        }

        public Graph CreateGraph()
        {
            return graphData.ToGraph();
        }

        public void SaveGraph(Graph graph)
        {
            // This would need to be implemented if we add serialization to the Graph class
            // For now, we'll use the editor to manually create the graph data
        }

        public void ClearGraph()
        {
            graphData.nodes.Clear();
            graphData.edges.Clear();

            // Add default Start and Goal nodes
            graphData.nodes.Add(new NodeData(new Vector2Int(0, 0), NodeType.Start));
            graphData.nodes.Add(new NodeData(new Vector2Int(2, 0), NodeType.Goal));
        }
    }
} 