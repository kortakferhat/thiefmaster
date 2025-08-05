using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Graph
{
    [Serializable]
    public class NodeData
    {
        public Vector2Int id;
        public NodeType type;
        public bool isDestroyed;

        public NodeData(Vector2Int id, NodeType type, bool isDestroyed = false)
        {
            this.id = id;
            this.type = type;
            this.isDestroyed = isDestroyed;
        }

        public Node ToNode()
        {
            var node = new Node(id, type);
            if (isDestroyed)
                node.Destroy();
            return node;
        }

        public Vector2 GetWorldPosition(float gridSize = 50f)
        {
            return new Vector2(id.x * gridSize, -id.y * gridSize);
        }
    }

    [Serializable]
    public class EdgeData
    {
        public Vector2Int fromId;
        public Vector2Int toId;
        public EdgeType type;
        public bool isUsed;

        public EdgeData(Vector2Int fromId, Vector2Int toId, EdgeType type, bool isUsed = false)
        {
            this.fromId = fromId;
            this.toId = toId;
            this.type = type;
            this.isUsed = isUsed;
        }
    }

    [Serializable]
    public class GraphData
    {
        public List<NodeData> nodes = new();
        public List<EdgeData> edges = new();
        public string graphName = "New Graph";

        public GraphData() { }

        public GraphData(Graph graph)
        {
            // This would need to be implemented if we add serialization to the Graph class
        }

        public Graph ToGraph()
        {
            var graph = new Graph();
            
            // Create nodes first
            foreach (var nodeData in nodes)
            {
                var node = nodeData.ToNode();
                graph.AddNode(node);
            }

            // Create connections between nodes
            foreach (var edgeData in edges)
            {
                graph.AddConnection(edgeData.fromId, edgeData.toId, edgeData.type);
                
                // If edge was used, mark the connection as used
                if (edgeData.isUsed)
                {
                    var connection = graph.GetConnection(edgeData.fromId, edgeData.toId - edgeData.fromId);
                    connection?.MarkAsUsed();
                }
            }

            return graph;
        }
    }
} 