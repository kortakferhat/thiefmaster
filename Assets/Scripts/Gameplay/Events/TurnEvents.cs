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
        public int RemainingMoves { get; }

        public TurnStartedEvent(int turnNumber, int remainingMoves)
        {
            TurnNumber = turnNumber;
            RemainingMoves = remainingMoves;
        }
    }
    
    /// <summary>
    /// Event triggered when a turn is completed
    /// </summary>
    public class TurnCompletedEvent : ITurnEvent
    {
        public int TurnNumber { get; }
        public int RemainingMoves { get; }
        
        public TurnCompletedEvent(int turnNumber, int remainingMoves)
        {
            TurnNumber = turnNumber;
            RemainingMoves = remainingMoves;
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
    
    /// <summary>
    /// Event triggered when an enemy is eliminated
    /// </summary>
    public class EnemyEliminatedEvent : ITurnEvent
    {
        public Vector2Int EnemyNodeId { get; }
        public Vector2Int PlayerNodeId { get; }
        public int TurnNumber { get; }
        
        public EnemyEliminatedEvent(Vector2Int enemyNodeId, Vector2Int playerNodeId, int turnNumber)
        {
            EnemyNodeId = enemyNodeId;
            PlayerNodeId = playerNodeId;
            TurnNumber = turnNumber;
        }
    }
}