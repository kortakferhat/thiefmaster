using System;

namespace TowerClicker.Infrastructure
{
    public enum GameState
    {
        Initializing,
        MainMenu,
        Game,
        Pause,
        GameOver,
        Win,
        Lose
    }
    
    public interface IGameManager : IService
    {
        GameState State { get; set; }
        public event Action<GameState> OnStateChanged;
        void Initialize();
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void EndGame();
        void WinGame();
        void LoseGame();
        void RestartGame();
    }
}