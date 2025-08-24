using System;

namespace Infrastructure
{
    public interface ITurnManager : IService
    {
        int CurrentTurn { get; }
        int RemainingMoves { get; }
        bool IsTurnInProgress { get; }
        void Initialize();
        void StartNextTurn();
        void CompleteTurn();
        void ResetTurns();
        void ResetForNewLevel();
    }
}