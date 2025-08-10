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
    
    /// <summary>
    /// Event triggered when game over occurs (player detected or eliminated)
    /// </summary>
    public class GameOverEvent : ITurnEvent
    {
        public int TurnNumber { get; }
        
        public GameOverEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }
    
    /// <summary>
    /// Event triggered when player wins (reaches goal node)
    /// </summary>
    public class WinEvent : ITurnEvent
    {
        public int TurnNumber { get; }
        public Vector2Int GoalNodeId { get; }
        
        public WinEvent(int turnNumber, Vector2Int goalNodeId)
        {
            TurnNumber = turnNumber;
            GoalNodeId = goalNodeId;
        }
    }
    
    /// <summary>
    /// Event triggered when player loses (enemy contact or detection)
    /// </summary>
    public class LoseEvent : ITurnEvent
    {
        public int TurnNumber { get; }
        public LoseReason Reason { get; }
        public Vector2Int EnemyNodeId { get; }
        
        public LoseEvent(int turnNumber, LoseReason reason, Vector2Int enemyNodeId = default)
        {
            TurnNumber = turnNumber;
            Reason = reason;
            EnemyNodeId = enemyNodeId;
        }
    }
    
    /// <summary>
    /// Reasons why the player can lose
    /// </summary>
    public enum LoseReason
    {
        EnemyContact,      // Player moved to enemy's node
        EnemyDetection,    // Player was seen by enemy vision
        InvalidMove,       // Player tried to move to invalid location
        TimeOut           // Turn limit exceeded (if implemented)
    }
}