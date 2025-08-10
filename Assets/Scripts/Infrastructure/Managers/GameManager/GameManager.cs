using System;
using System.Collections;
using UnityEngine;
using Gameplay.Events;
using Infrastructure;
using Infrastructure.Managers.LevelManager;

namespace TowerClicker.Infrastructure
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        private GameState _state;

        public GameState State
        {
            get => _state;
            set
            {
                _state = value;
                OnStateChanged?.Invoke(_state);
            }
        }

        public event Action<GameState> OnStateChanged;

        public void Initialize()
        {
            State = GameState.Initializing;
            
            // Subscribe to win/lose events
            EventBus.Subscribe<WinEvent>(OnPlayerWon);
            EventBus.Subscribe<LoseEvent>(OnPlayerLost);
            
            // Initialization logic here
        }
        
        private void OnPlayerWon(WinEvent winEvent)
        {
            Debug.Log($"[GameManager] Player won at turn {winEvent.TurnNumber}, goal node: {winEvent.GoalNodeId}");
            State = GameState.Win;
        }
        
        private void OnPlayerLost(LoseEvent loseEvent)
        {
            Debug.Log($"[GameManager] Player lost at turn {loseEvent.TurnNumber}, reason: {loseEvent.Reason}, enemy node: {loseEvent.EnemyNodeId}");
            State = GameState.Lose;
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
        
        public void WinGame()
        {
            State = GameState.Win;
            Debug.Log("[GameManager] Game won!");
        }
        
        public void LoseGame()
        {
            State = GameState.Lose;
            Debug.Log("[GameManager] Game lost!");
            
            // Auto-restart after a short delay
            StartCoroutine(AutoRestartAfterDelay(2f));
        }
        
        private IEnumerator AutoRestartAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            RestartGame();
        }
        
        public void RestartGame()
        {
            Debug.Log("[GameManager] Restarting game...");
            State = GameState.Game;
            
            ServiceLocator.Get<ILevelManager>().RestartLevel();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<WinEvent>(OnPlayerWon);
            EventBus.Unsubscribe<LoseEvent>(OnPlayerLost);
        }
    }
}