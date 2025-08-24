using Cysharp.Threading.Tasks;
using Gameplay.Events;
using Infrastructure.Managers.LevelManager;
using UnityEngine;

namespace Infrastructure.Managers.GameManager
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        private GameState _state;

        public GameState State => _state;

        private const float LoseWaitTime = 1f;
        private const float RestartGameDelay = 2f;
        
        public void Initialize()
        {
            SetGameState(GameState.Initializing);
            
            // Subscribe to win/lose events
        }
        
        public void StartGame()
        {
            SetGameState(GameState.Game);
            // Start game logic here
        }

        public void PauseGame()
        {
            SetGameState(GameState.Pause);
            // Pause game logic here
        }
        
        public void WinGame()
        {
            SetGameState(GameState.Finish, GameEvents.GameEventChangeReason.Win);
            Debug.Log("[GameManager] Game won!");
        }
        
        public async void LoseGame()
        {
            await UniTask.WaitForSeconds(LoseWaitTime, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();

            SetGameState(GameState.Finish, GameEvents.GameEventChangeReason.Lose);
            Debug.Log("[GameManager] Game lost!");
            
            await UniTask.WaitForSeconds(RestartGameDelay, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            RestartGame();
        }
        
        public void RestartGame()
        {
            Debug.Log("[GameManager] Restarting game...");
            SetGameState(GameState.Game);
            
            ServiceLocator.Get<ILevelManager>().RestartLevel();
        }

        private void SetGameState(GameState state, GameEvents.GameEventChangeReason reason = GameEvents.GameEventChangeReason.None)
        {
            EventBus.Publish(new GameEvents.GameStateChangeEvent(_state, state, reason));
            _state = state;
        }
        
        private void OnDestroy()
        {
        }
    }
}