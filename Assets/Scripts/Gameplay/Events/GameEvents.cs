using Infrastructure;

namespace Gameplay.Events
{
    public class GameEvents
    {
        public class GameStateChangeEvent : IBusEvent
        {
            public GameState PreviousState { get; }
            public GameState CurrentState { get; }
            public GameEventChangeReason Reason { get; }

            public GameStateChangeEvent(GameState prev, GameState currentState, GameEventChangeReason reason = GameEventChangeReason.None)
            {
                PreviousState = prev;
                CurrentState = currentState;
                
                Reason = reason;
            }
        }
        
        public enum GameEventChangeReason : byte
        {
            None,
            Lose,
            Win
        }
    }
}