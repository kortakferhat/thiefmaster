using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Graph
{
    public class Graph
    {
        private readonly Dictionary<Vector2Int, Node> nodes = new();

        public void AddNode(Node node) => nodes[node.Id] = node;

        public Node GetNode(Vector2Int id) => nodes.GetValueOrDefault(id);

        public Node GetStartNode()
        {
            return nodes.Values.FirstOrDefault(node => node.Type == NodeType.Start);
        }

        /// <summary>
        /// Add a bidirectional connection between two nodes
        /// O(1) operation - much faster than the old AddEdge + GetEdge pattern
        /// </summary>
        public void AddConnection(Vector2Int fromId, Vector2Int toId, EdgeType connectionType)
        {
            var fromNode = GetNode(fromId);
            var toNode = GetNode(toId);
            
            if (fromNode == null || toNode == null)
            {
                Debug.LogWarning($"Cannot create connection between {fromId} and {toId} - one or both nodes don't exist");
                return;
            }
            
            // Calculate direction from fromNode to toNode
            var direction = toId - fromId;
            var reverseDirection = fromId - toId;
            
            // Create connections
            var forwardConnection = new NodeConnection(toId, connectionType);
            var backwardConnection = new NodeConnection(fromId, connectionType);
            
            // Set target node references
            forwardConnection.SetTargetNode(toNode);
            backwardConnection.SetTargetNode(fromNode);
            
            // Add connections to nodes
            fromNode.AddConnection(direction, forwardConnection);
            
            // For non-directed edges, add reverse connection
            if (connectionType != EdgeType.Directed)
            {
                toNode.AddConnection(reverseDirection, backwardConnection);
            }
        }

        /// <summary>
        /// Check if movement from one node to another is valid
        /// O(1) operation - replaces the expensive GetEdge method!
        /// </summary>
        public bool CanMoveFromTo(Vector2Int fromId, Vector2Int toId)
        {
            var fromNode = GetNode(fromId);
            if (fromNode == null) return false;
            
            var direction = toId - fromId;
            return fromNode.CanMoveTo(direction);
        }

        /// <summary>
        /// Get the connection type between two nodes
        /// O(1) operation
        /// </summary>
        public NodeConnection GetConnection(Vector2Int fromId, Vector2Int direction)
        {
            var fromNode = GetNode(fromId);
            return fromNode?.GetConnection(direction);
        }

        /// <summary>
        /// Get all nodes in the graph
        /// </summary>
        public IEnumerable<Node> GetAllNodes()
        {
            return nodes.Values;
        }
    }
}