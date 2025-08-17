using System;

namespace TowerClicker.Infrastructure
{
    public enum GameState
    {
        Initializing,
        MainMenu,
        Game,
        Pause,
        Finish,
    }
    
    public interface IGameManager : IService
    {
        GameState State { get; set; }
        public event Action<GameState> OnStateChanged;
        void Initialize();
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void FinishGame();
        void WinGame();
        void LoseGame();
        void RestartGame();
    }
}