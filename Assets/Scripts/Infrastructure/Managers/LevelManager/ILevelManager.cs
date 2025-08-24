using System;
using Gameplay.Graph;
using UnityEngine;

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
        event Action<Graph> OnGridGenerated;
        event Action<Graph> OnGridInstantiated;
        
        System.Threading.Tasks.Task Initialize();
        void LoadLevel(int levelIndex);
        void LoadLevel(GraphScriptableObject levelGraph);
        void CompleteLevel();
        void FailLevel();
        void RestartLevel();
        void NextLevel();
        void SetGridConfig(GridConfig config);
        void ResetLevelProgress();
        
        // Grid Position Methods
        Vector3 GetNodeWorldPosition(Vector2Int nodeId);
        Vector3 GetNodeActualWorldPosition(Vector2Int nodeId);
        bool TryMoveToNode(Vector2Int currentNodeId, Vector2Int direction, out Vector2Int targetNodeId);
        
        // Grid Configuration Methods
        GridConfig GetGridConfig();
        
        // Graph Access Methods
        Graph GetCurrentGraph();
    }
} 