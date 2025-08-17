using TowerClicker.Infrastructure;

namespace Gameplay.Events
{
    public class GameEvents
    {
        public class GameStateChangeEvent : IBusEvent
        {
            public GameState CurrentState { get; }

            public GameStateChangeEvent(GameState currentState)
            {
                CurrentState = currentState;
            }
        }
    }
}