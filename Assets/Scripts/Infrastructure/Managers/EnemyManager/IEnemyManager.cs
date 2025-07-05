using UnityEngine;

namespace TowerClicker.Infrastructure
{
    public interface IEnemyManager : IService
    {
        public void Initialize(IPoolManager poolManager, IGameManager gameManager);
        public void SpawnEnemy(Vector3 position, Quaternion rotation);
        public void DespawnEnemy(GameObject enemy);
        public void UpdateEnemyCount(int count);
    }
}