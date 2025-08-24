using System;

namespace Infrastructure
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
        GameState State { get; }
        void Initialize();
        void StartGame();
        void PauseGame();
        void WinGame();
        void LoseGame();
        void RestartGame();
    }
}