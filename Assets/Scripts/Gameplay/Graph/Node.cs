using UnityEngine;

namespace Gameplay.Graph
{
    public enum NodeType { Normal, Start, Goal, Breakable, Redirector, Trap, Enemy }

    public class Node
    {
        public Vector2Int Id { get; }
        public NodeType Type { get; set; }
        public bool IsDestroyed { get; private set; }

        public Node(Vector2Int id, NodeType type)
        {
            Id = id;
            Type = type;
        }

        public void Destroy()
        {
            IsDestroyed = true;
        }
    }
}