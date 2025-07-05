using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Enemy
{
    public static class EnemyUtils
    {
        public static Enemy GetNearestEnemy(Vector3 target)
        {
            var poolManager = ServiceLocator.Get<IPoolManager>();
            var allEnemies = poolManager.GetActivePoolObjects(PoolKeys.Enemy);
            Enemy nearestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in allEnemies)
            {
                var distance = Vector3.Distance(enemy.transform.position, target);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy.GetComponent<Enemy>();
                }
            }

            return nearestEnemy;
        }
    }
}