using System;
using System.Collections;
using Cysharp.Threading.Tasks;
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
                EventBus.Publish(new GameEvents.GameStateChangeEvent(_state));
                OnStateChanged?.Invoke(_state);
            }
        }

        public event Action<GameState> OnStateChanged;

        private const float LoseWaitTime = 1f;
        private const float RestartGameDelay = 2f;
        
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
            State = GameState.Finish;
        }
        
        private void OnPlayerLost(LoseEvent loseEvent)
        {
            Debug.Log($"[GameManager] Player lost at turn {loseEvent.TurnNumber}, reason: {loseEvent.Reason}, enemy node: {loseEvent.EnemyNodeId}");
            LoseGame();
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

        public void FinishGame()
        {
            State = GameState.Finish;
            // End game logic here
        }
        
        public void WinGame()
        {
            State = GameState.Finish;
            Debug.Log("[GameManager] Game won!");
        }
        
        public async void LoseGame()
        {
            await UniTask.WaitForSeconds(LoseWaitTime, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();

            State = GameState.Finish;
            Debug.Log("[GameManager] Game lost!");
            
            await UniTask.WaitForSeconds(RestartGameDelay, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
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