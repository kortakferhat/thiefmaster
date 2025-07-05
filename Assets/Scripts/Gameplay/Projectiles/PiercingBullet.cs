using System.Collections.Generic;
using Infrastructure.Managers.PoolManager;
using Gameplay.Projectiles;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public class PiercingBullet : BaseBullet
    {
        [Header("Piercing Settings")]
        [SerializeField] private int maxPierces = 5;
        [SerializeField] private float damageReductionPerPierce = 0.1f; // 10% damage reduction per pierce
        
        private int currentPierces = 0;
        private HashSet<Gameplay.Enemy.Enemy> hitEnemies = new HashSet<Gameplay.Enemy.Enemy>();
        private int currentDamage;

        protected override string GetDefaultPoolKey()
        {
            return PoolKeys.PiercingBullet;
        }

        protected override void OnBulletFired(Vector3 target)
        {
            // Reset pierce data when bullet is fired
            currentPierces = 0;
            currentDamage = damage;
            hitEnemies.Clear();
        }

        protected override void OnEnemyHit(Collider enemyCollider, Gameplay.Enemy.Enemy enemy)
        {
            // Check if we already hit this specific enemy
            if (hitEnemies.Contains(enemy))
                return;

            // Add enemy to hit set
            hitEnemies.Add(enemy);
            currentPierces++;

            // Apply damage with current damage value
            enemy.OnHit(currentDamage);

            // Reduce damage for next pierce
            if (damageReductionPerPierce > 0)
            {
                currentDamage = Mathf.Max(1, Mathf.RoundToInt(currentDamage * (1f - damageReductionPerPierce)));
            }

            // Check if we've reached maximum pierces
            if (currentPierces >= maxPierces)
            {
                Despawn();
                return;
            }

            // Continue flying through the enemy (don't change direction or despawn)
        }

        protected override void HandleEnemyHit(Collider enemyCollider)
        {
            // Custom hit handling that doesn't automatically play particle effects
            // since we might hit the same area multiple times
            var enemy = enemyCollider.GetComponent<Gameplay.Enemy.Enemy>();
            
            // Only play particle effect if this is a new enemy hit
            if (!hitEnemies.Contains(enemy))
            {
                var offset = -rb.linearVelocity.normalized * 0.65f;
                particleManager.PlayParticle(PoolKeys.HitVFX, enemyCollider.transform.position + offset, Quaternion.identity);
            }

            OnEnemyHit(enemyCollider, enemy);
        }

        protected override bool ShouldDespawnOnNonTowerHit(Collider other)
        {
            // Pierce bullets should despawn on non-tower hits like walls or boundaries
            return true;
        }

        protected override void OnBeforeDespawn()
        {
            // Clear hit enemies set when despawning
            hitEnemies.Clear();
            currentPierces = 0;
            currentDamage = damage;
        }

        // Add visual feedback for piercing
        protected override void OnBulletUpdate()
        {
            // Optional: Add trail effects or other visual indicators
            // that this bullet can pierce through enemies
        }
    }
} 