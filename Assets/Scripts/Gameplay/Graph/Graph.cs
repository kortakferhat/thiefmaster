using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Graph
{
    public class Graph
    {
        private readonly Dictionary<Vector2Int, Node> nodes = new();
        private readonly List<Edge> edges = new();

        public void AddNode(Node node) => nodes[node.Id] = node;
        public void AddEdge(Edge edge) => edges.Add(edge);

        public Node GetNode(Vector2Int id) => nodes.GetValueOrDefault(id);

        public Node GetStartNode()
        {
            return nodes.Values.FirstOrDefault(node => node.Type == NodeType.Start);
        }

        public Edge GetEdge(Node from, Vector2Int direction)
        {
            var targetId = from.Id + direction;
            
            foreach (var edge in edges)
            {
                if ((edge.From == from && edge.To.Id == targetId && !edge.To.IsDestroyed) ||
                    (edge.To == from && edge.From.Id == targetId && !edge.From.IsDestroyed))
                {
                    return edge;
                }
            }
            
            return null;
        }
    }

}