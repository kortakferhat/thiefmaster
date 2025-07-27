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

        public Edge GetEdge(Node from, Vector2Int direction)
        {
            var targetId = from.Id + direction;
            return edges.FirstOrDefault(e =>
                e.From == from && e.To.Id == targetId && !e.IsUsed &&
                !e.To.IsDestroyed);
        }
    }

}