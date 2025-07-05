using System.Collections.Generic;
using Gameplay.Enemy;
using Infrastructure.Managers.PoolManager;
using UnityEngine;

namespace TowerClicker.Infrastructure
{
    public class EnemyManager : MonoBehaviour, IEnemyManager
    {
        private IPoolManager _poolManager;
        private IGameManager _gameManager;
        private bool initialized = false;
        
        private int activeEnemyCount = 0;
        private int maxEnemies = 5;
        private float spawnInterval = 2f;
        private float spawnTimer;
        private int spawnedEnemiesCount = 0;
        private readonly List<Vector3> enemySpawnPositions = new List<Vector3>
        {
            new(19, -1, 0),
            new(-19, -1, 0),
            new(0, -1, 19),
            new(0, -1, -19),
        };

        public void Initialize(IPoolManager poolManager, IGameManager gameManager)
        {
            _poolManager = poolManager;
            _gameManager = gameManager;
            initialized = true;
        }
        
        private void FixedUpdate()
        {
            if (!initialized)
            {
                return;
            }

            if (_gameManager.State != GameState.Game)
            {
                return;
            }
            
            if (spawnTimer > 0)
            {
                spawnTimer -= Time.fixedDeltaTime;
                return;
            }
            
            if (activeEnemyCount >= maxEnemies)
            {
                return;
            }

            SpawnEnemy(enemySpawnPositions[Random.Range(0, enemySpawnPositions.Count)], Quaternion.identity);
        }

        public void SpawnEnemy(Vector3 position, Quaternion rotation)
        {
            if (!initialized)
            {
                return;
            }

            var enemyGo = _poolManager.Spawn(PoolKeys.Enemy, position, rotation);
            enemyGo.transform.position = position;
            enemyGo.transform.rotation = rotation;
            
            var enemy = enemyGo.GetComponent<Enemy>();
            enemy.Initialize();
            
            spawnTimer = spawnInterval;
            spawnedEnemiesCount++;
            activeEnemyCount++;
        }

        public void DespawnEnemy(GameObject enemy)
        {
            if (!initialized)
            {
                return;
            }
            
            _poolManager.Despawn(PoolKeys.Enemy, enemy);
            activeEnemyCount--;
        }

        public void UpdateEnemyCount(int count)
        {

        }
    }
}