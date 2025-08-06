using UnityEngine;

namespace Gameplay.Events
{
    /// <summary>
    /// Base interface for all turn-related events
    /// </summary>
    public interface ITurnEvent : IBusEvent { }
    
    /// <summary>
    /// Event triggered when a new turn starts
    /// </summary>
    public class TurnStartedEvent : ITurnEvent
    {
        public int TurnNumber { get; }
        
        public TurnStartedEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }
    
    /// <summary>
    /// Event triggered when a turn is completed
    /// </summary>
    public class TurnCompletedEvent : ITurnEvent
    {
        public int TurnNumber { get; }
        
        public TurnCompletedEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }
    
    /// <summary>
    /// Event triggered when player moves (triggers turn progression)
    /// </summary>
    public class PlayerMovedEvent : ITurnEvent
    {
        public Vector2Int FromNodeId { get; }
        public Vector2Int ToNodeId { get; }
        public int TurnNumber { get; }
        
        public PlayerMovedEvent(Vector2Int fromNodeId, Vector2Int toNodeId, int turnNumber)
        {
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            TurnNumber = turnNumber;
        }
    }
}