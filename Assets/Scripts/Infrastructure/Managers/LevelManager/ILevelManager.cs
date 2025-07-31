using System;
using Gameplay.Graph;
using TowerClicker.Infrastructure;

namespace Infrastructure.Managers.LevelManager
{
    public interface ILevelManager : IService
    {
        int CurrentLevel { get; }
        GraphScriptableObject CurrentLevelGraph { get; }
        
        event Action<int> OnLevelChanged;
        event Action<GraphScriptableObject> OnLevelLoaded;
        event Action OnLevelCompleted;
        event Action OnLevelFailed;
        
        System.Threading.Tasks.Task Initialize();
        void LoadLevel(int levelIndex);
        void LoadLevel(GraphScriptableObject levelGraph);
        void CompleteLevel();
        void FailLevel();
        void RestartLevel();
        void NextLevel();
        void SetGridConfig(GridConfig config);
        void ResetLevelProgress();
        
        // Level Rotation Methods
        void SetLevelRotation(LevelRotation rotation);
        LevelRotation GetCurrentRotation();
    }
} 