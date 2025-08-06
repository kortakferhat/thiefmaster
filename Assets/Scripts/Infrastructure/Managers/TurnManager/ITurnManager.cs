using System;

namespace TowerClicker.Infrastructure
{
    public interface ITurnManager : IService
    {
        int CurrentTurn { get; }
        bool IsTurnInProgress { get; }
        
        event Action<int> OnTurnStarted;
        event Action<int> OnTurnCompleted;
        
        void Initialize();
        void StartNextTurn();
        void CompleteTurn();
        void ResetTurns();
    }
}