namespace Gameplay.Graph
{
public enum EdgeType { Standard, Directed, Slippery, Breakable }

public class Edge
{
    public Node From { get; }
    public Node To { get; }
    public EdgeType Type { get; }
    public bool IsUsed { get; private set; }

    public bool IsDirected => Type == EdgeType.Directed;
    public bool IsSlippery => Type == EdgeType.Slippery;
    public bool IsBreakable => Type == EdgeType.Breakable;

    public Edge(Node from, Node to, EdgeType type)
    {
        From = from;
        To = to;
        Type = type;
    }

    public void MarkAsUsed()
    {
        if (IsBreakable)
            IsUsed = true;
    }
}

}