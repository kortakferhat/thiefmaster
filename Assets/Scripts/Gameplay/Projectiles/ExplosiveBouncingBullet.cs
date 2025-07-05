using System.Collections.Generic;
using Infrastructure.Managers.PoolManager;
using Gameplay.Projectiles;
using Gameplay.Enemy;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public class ExplosiveBouncingBullet : BaseBullet
    {
        [Header("Bouncing Settings")]
        [SerializeField] private int maxBounces = 2;
        [SerializeField] private float bounceRange = 4f;
        [SerializeField] private float bounceSpeedMultiplier = 1.1f;
        
        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private int explosionDamage = 2;
        [SerializeField] private string explosionVFXKey = "TowerExplosion";
        
        private int currentBounces = 0;
        private List<Gameplay.Enemy.Enemy> hitEnemies = new List<Gameplay.Enemy.Enemy>();

        protected override string GetDefaultPoolKey()
        {
            return PoolKeys.ExplosiveBouncingBullet;
        }

        protected override void OnBulletFired(Vector3 target)
        {
            // Reset bounce data when bullet is fired
            currentBounces = 0;
            hitEnemies.Clear();
        }

        protected override void OnEnemyHit(Collider enemyCollider, Gameplay.Enemy.Enemy enemy)
        {
            // Create explosion on hit
            CreateExplosion(enemyCollider.transform.position);
            
            // Add enemy to hit list to prevent hitting the same enemy twice in bounces
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

        private void CreateExplosion(Vector3 explosionCenter)
        {
            // Play explosion VFX
            particleManager.PlayParticle(explosionVFXKey, explosionCenter, Quaternion.identity);
            
            // Find all enemies within explosion radius
            var poolManager = ServiceLocator.Get<IPoolManager>();
            var allEnemies = poolManager.GetActivePoolObjects(PoolKeys.Enemy);

            foreach (var enemyObj in allEnemies)
            {
                float distance = Vector3.Distance(enemyObj.transform.position, explosionCenter);
                
                if (distance <= explosionRadius)
                {
                    var enemy = enemyObj.GetComponent<Gameplay.Enemy.Enemy>();
                    
                    // Calculate damage falloff based on distance
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    int finalDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);
                    finalDamage = Mathf.Max(1, finalDamage); // Minimum 1 damage
                    
                    enemy.OnHit(finalDamage);
                    
                    // Play hit effect for each enemy in explosion
                    particleManager.PlayParticle(PoolKeys.HitVFX, enemyObj.transform.position, Quaternion.identity);
                }
            }
        }

        private Gameplay.Enemy.Enemy FindNextBounceTarget(Vector3 fromPosition)
        {
            var poolManager = ServiceLocator.Get<IPoolManager>();
            var allEnemies = poolManager.GetActivePoolObjects(PoolKeys.Enemy);
            Gameplay.Enemy.Enemy bestTarget = null;
            float minDistance = float.MaxValue;

            foreach (var enemyObj in allEnemies)
            {
                var enemy = enemyObj.GetComponent<Gameplay.Enemy.Enemy>();
                
                // Skip if we already hit this enemy in a bounce
                if (hitEnemies.Contains(enemy))
                    continue;

                float distance = Vector3.Distance(enemyObj.transform.position, fromPosition);
                
                // Check if enemy is within bounce range
                if (distance <= bounceRange && distance < minDistance)
                {
                    minDistance = distance;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }

        private void BounceToTarget(Gameplay.Enemy.Enemy target)
        {
            Vector3 newDirection = (target.transform.position - rb.position).normalized;
            
            // Look at the new target
            rb.transform.LookAt(target.transform.position);
            
            // Set new velocity with speed increase
            rb.linearVelocity = newDirection * speed * bounceSpeedMultiplier;
            
            // Update direction
            direction = newDirection;
        }

        protected override void HandleEnemyHit(Collider enemyCollider)
        {
            // Override to prevent normal hit effects since we handle explosion
            var enemy = enemyCollider.GetComponent<Gameplay.Enemy.Enemy>();
            OnEnemyHit(enemyCollider, enemy);
        }

        protected override void OnBeforeDespawn()
        {
            // Clear hit enemies list when despawning
            hitEnemies.Clear();
            currentBounces = 0;
        }

        protected override bool ShouldDespawnOnNonTowerHit(Collider other)
        {
            // Create explosion even on non-tower hits (like walls)
            CreateExplosion(transform.position);
            return true;
        }
    }
} 