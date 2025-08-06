using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gameplay.Enemy;
using Infrastructure.Managers.LevelManager;

namespace TowerClicker.Infrastructure
{
    public interface IGridEnemyManager : IService
    {
        IReadOnlyList<GridEnemy> ActiveEnemies { get; }
        
        void Initialize(ILevelManager levelManager, IGameManager gameManager);
        void SpawnEnemyAtNode(Vector2Int nodeId, Vector2Int facingDirection = default);
        void RemoveEnemy(GridEnemy enemy);
        void ClearAllEnemies();
        GridEnemy GetEnemyAtNode(Vector2Int nodeId);
        bool IsNodeOccupiedByEnemy(Vector2Int nodeId);
        System.Collections.Generic.IEnumerable<Vector2Int> GetAllEnemyPositions();
    }
}