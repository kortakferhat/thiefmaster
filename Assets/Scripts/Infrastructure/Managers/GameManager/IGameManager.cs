using System;

namespace TowerClicker.Infrastructure
{
    public enum GameState
    {
        Initializing,
        MainMenu,
        Game,
        Pause,
        GameOver
    }
    
    public interface IGameManager : IService
    {
        GameState State { get; }
        public event Action<GameState> OnStateChanged;
        void Initialize();
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void EndGame();
    }
}