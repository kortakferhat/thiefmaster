using UnityEngine;

namespace Gameplay.Graph
{
    /// <summary>
    /// Represents a connection from a node to its neighbor
    /// Replaces the separate Edge system with direct node connections
    /// </summary>
    [System.Serializable]
    public class NodeConnection
    {
        [SerializeField] private Vector2Int _targetNodeId;
        [SerializeField] private EdgeType _connectionType;
        [SerializeField] private bool _isUsed;
        
        // Runtime reference to the actual target node (set during graph construction)
        private Node _targetNode;
        
        public Vector2Int TargetNodeId => _targetNodeId;
        public EdgeType ConnectionType => _connectionType;
        public bool IsUsed => _isUsed;
        public Node TargetNode => _targetNode;
        
        public bool IsDirected => _connectionType == EdgeType.Directed;
        public bool IsSlippery => _connectionType == EdgeType.Slippery;
        public bool IsBreakable => _connectionType == EdgeType.Breakable;
        
        public NodeConnection(Vector2Int targetNodeId, EdgeType connectionType)
        {
            _targetNodeId = targetNodeId;
            _connectionType = connectionType;
            _isUsed = false;
        }
        
        public void SetTargetNode(Node node)
        {
            _targetNode = node;
        }
        
        public void MarkAsUsed()
        {
            if (IsBreakable)
                _isUsed = true;
        }
        
        public bool CanTraverse()
        {
            return _targetNode != null && !_targetNode.IsDestroyed && (!IsBreakable || !_isUsed);
        }
    }
}