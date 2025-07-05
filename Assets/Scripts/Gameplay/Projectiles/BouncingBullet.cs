using System.Collections.Generic;
using Infrastructure.Managers.PoolManager;
using Gameplay.Projectiles;
using Gameplay.Enemy;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public class BouncingBullet : BaseBullet
    {
        [Header("Bouncing Settings")]
        [SerializeField] private int maxBounces = 3;
        [SerializeField] private float bounceRange = 5f;
        [SerializeField] private float bounceSpeedMultiplier = 1.2f;
        
        private int currentBounces = 0;
        private List<Gameplay.Enemy.Enemy> hitEnemies = new List<Gameplay.Enemy.Enemy>();

        protected override string GetDefaultPoolKey()
        {
            return PoolKeys.BouncingBullet;
        }

        protected override void OnBulletFired(Vector3 target)
        {
            // Reset bounce data when bullet is fired
            currentBounces = 0;
            hitEnemies.Clear();
        }

        protected override void OnEnemyHit(Collider enemyCollider, Gameplay.Enemy.Enemy enemy)
        {
            // Add enemy to hit list to prevent hitting the same enemy twice
            hitEnemies.Add(enemy);
            currentBounces++;

            // Check if we can bounce to another enemy
            if (currentBounces < maxBounces)
            {
                var nextTarget = FindNextBounceTarget(enemyCollider.transform.position);
                if (nextTarget != null)
                {
                    BounceToTarget(nextTarget);
                    return;
                }
            }

            // No more bounces available or no valid target found
            Despawn();
        }

        private Gameplay.Enemy.Enemy FindNextBounceTarget(Vector3 fromPosition)
        {
            // Use EnemyUtils but filter for valid bounce targets
            var allEnemies = new List<Gameplay.Enemy.Enemy>();
            var poolManager = ServiceLocator.Get<IPoolManager>();
            var enemyObjects = poolManager.GetActivePoolObjects(PoolKeys.Enemy);

            foreach (var enemyObj in enemyObjects)
            {
                var enemy = enemyObj.GetComponent<Gameplay.Enemy.Enemy>();
                
                // Skip if we already hit this enemy
                if (hitEnemies.Contains(enemy))
                    continue;

                float distance = Vector3.Distance(enemyObj.transform.position, fromPosition);
                
                // Check if enemy is within bounce range
                if (distance <= bounceRange)
                {
                    allEnemies.Add(enemy);
                }
            }

            // Find the nearest enemy from valid bounce targets
            if (allEnemies.Count == 0)
                return null;

            Gameplay.Enemy.Enemy nearestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in allEnemies)
            {
                float distance = Vector3.Distance(enemy.transform.position, fromPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }

        private void BounceToTarget(Gameplay.Enemy.Enemy target)
        {
            Vector3 newDirection = (target.transform.position - rb.position).normalized;
            
            // Look at the new target
            rb.transform.LookAt(target.transform.position);
            
            // Set new velocity with potential speed increase
            rb.linearVelocity = newDirection * speed * bounceSpeedMultiplier;
            
            // Update direction
            direction = newDirection;
        }

        protected override void OnBeforeDespawn()
        {
            // Clear hit enemies list when despawning
            hitEnemies.Clear();
            currentBounces = 0;
        }

        protected override bool ShouldDespawnOnNonTowerHit(Collider other)
        {
            // Only despawn on non-tower hit if we have no more bounces
            return currentBounces >= maxBounces;
        }
    }
} 