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
            
            // Create nodes
            var nodeDict = new Dictionary<Vector2Int, Node>();
            foreach (var nodeData in nodes)
            {
                var node = nodeData.ToNode();
                nodeDict[nodeData.id] = node;
                graph.AddNode(node);
            }

            // Create edges
            foreach (var edgeData in edges)
            {
                if (nodeDict.TryGetValue(edgeData.fromId, out var fromNode) && 
                    nodeDict.TryGetValue(edgeData.toId, out var toNode))
                {
                    var edge = new Edge(fromNode, toNode, edgeData.type);
                    if (edgeData.isUsed)
                        edge.MarkAsUsed();
                    graph.AddEdge(edge);
                }
            }

            return graph;
        }
    }
} 