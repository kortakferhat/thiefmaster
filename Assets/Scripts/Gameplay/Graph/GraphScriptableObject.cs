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
        public float nodeSize = 1f;
        public float edgeWidth = 0.1f;
        public Color startNodeColor = Color.green;
        public Color goalNodeColor = Color.red;
        public Color normalNodeColor = Color.white;
        public Color breakableNodeColor = Color.yellow;
        public Color redirectorNodeColor = Color.blue;
        public Color trapNodeColor = Color.black;
        public Color enemyNodeColor = Color.magenta;

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
        }
    }
} 