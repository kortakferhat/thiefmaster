using System;
using UnityEngine;

namespace TowerClicker.Infrastructure
{
    public class GameManager : IGameManager
    {
        private GameState _state;

        public GameState State
        {
            get => _state;
            private set
            {
                _state = value;
                OnStateChanged?.Invoke(_state);
            }
        }

        public event Action<GameState> OnStateChanged;

        public void Initialize()
        {
            State = GameState.Initializing;
            // Initialization logic here
        }

        public void StartGame()
        {
            State = GameState.Game;
            // Start game logic here
        }

        public void PauseGame()
        {
            State = GameState.Pause;
            // Pause game logic here
        }

        public void ResumeGame()
        {
            State = GameState.Game;
            // Resume game logic here
        }

        public void EndGame()
        {
            State = GameState.GameOver;
            // End game logic here
        }
    }
}