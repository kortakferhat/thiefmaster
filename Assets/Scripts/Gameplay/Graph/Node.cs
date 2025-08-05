using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Graph
{
    public enum NodeType { Normal, Start, Goal, Breakable, Redirector, Trap, Enemy }

    public class Node
    {
        public Vector2Int Id { get; }
        public NodeType Type { get; set; }
        public bool IsDestroyed { get; private set; }
        
        // Adjacency list - direct connections to neighboring nodes
        private readonly Dictionary<Vector2Int, NodeConnection> _connections = new();
        
        public Node(Vector2Int id, NodeType type)
        {
            Id = id;
            Type = type;
        }

        public void Destroy()
        {
            IsDestroyed = true;
        }
        
        /// <summary>
        /// Add a connection to a neighboring node
        /// O(1) operation
        /// </summary>
        public void AddConnection(Vector2Int direction, NodeConnection connection)
        {
            _connections[direction] = connection;
        }
        
        /// <summary>
        /// Get connection in a specific direction
        /// O(1) operation - replaces the expensive GetEdge lookup!
        /// </summary>
        public NodeConnection GetConnection(Vector2Int direction)
        {
            return _connections.GetValueOrDefault(direction);
        }
        
        /// <summary>
        /// Check if movement in direction is valid
        /// O(1) operation
        /// </summary>
        public bool CanMoveTo(Vector2Int direction)
        {
            var connection = GetConnection(direction);
            return connection?.CanTraverse() == true;
        }
        
        /// <summary>
        /// Get all valid connections from this node
        /// Useful for pathfinding algorithms
        /// </summary>
        public IEnumerable<(Vector2Int direction, NodeConnection connection)> GetValidConnections()
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Value.CanTraverse())
                    yield return (kvp.Key, kvp.Value);
            }
        }
        
        /// <summary>
        /// Get the target node in a specific direction
        /// O(1) operation
        /// </summary>
        public Node GetNeighbor(Vector2Int direction)
        {
            var connection = GetConnection(direction);
            return connection?.TargetNode;
        }
    }
}