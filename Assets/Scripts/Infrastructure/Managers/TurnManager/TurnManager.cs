using System;
using UnityEngine;
using Gameplay.Events;
using Infrastructure;

namespace TowerClicker.Infrastructure
{
    public class TurnManager : ITurnManager
    {
        private int _currentTurn = 0;
        private bool _isTurnInProgress = false;
        
        public int CurrentTurn => _currentTurn;
        public bool IsTurnInProgress => _isTurnInProgress;
        
        public event Action<int> OnTurnStarted;
        public event Action<int> OnTurnCompleted;
        
        public void Initialize()
        {
            _currentTurn = 0;
            _isTurnInProgress = false;
            
            Debug.Log("[TurnManager] Initialized - Ready for turn-based gameplay");
        }
        
        public void StartNextTurn()
        {
            if (_isTurnInProgress)
            {
                Debug.LogWarning("[TurnManager] Cannot start new turn - turn already in progress");
                return;
            }
            
            _currentTurn++;
            _isTurnInProgress = true;
            
            Debug.Log($"[TurnManager] Turn {_currentTurn} started");
            
            // Trigger events
            OnTurnStarted?.Invoke(_currentTurn);
            EventBus.Publish(new TurnStartedEvent(_currentTurn));
        }
        
        public void CompleteTurn()
        {
            if (!_isTurnInProgress)
            {
                Debug.LogWarning("[TurnManager] Cannot complete turn - no turn in progress");
                return;
            }
            
            _isTurnInProgress = false;
            
            Debug.Log($"[TurnManager] Turn {_currentTurn} completed");
            
            // Trigger events
            OnTurnCompleted?.Invoke(_currentTurn);
            EventBus.Publish(new TurnCompletedEvent(_currentTurn));
        }
        
        public void ResetTurns()
        {
            _currentTurn = 0;
            _isTurnInProgress = false;
            
            Debug.Log("[TurnManager] Turns reset");
        }
        
        public void ResetForNewLevel()
        {
            _currentTurn = 0;
            _isTurnInProgress = false;
            
            Debug.Log("[TurnManager] Reset for new level - turns reset to 0");
        }
    }
}