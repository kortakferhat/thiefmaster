using System;

namespace TowerClicker.Infrastructure
{
    public interface ITurnManager : IService
    {
        int CurrentTurn { get; }
        bool IsTurnInProgress { get; }
        void Initialize();
        void StartNextTurn();
        void CompleteTurn();
        void ResetTurns();
        void ResetForNewLevel();
    }
}