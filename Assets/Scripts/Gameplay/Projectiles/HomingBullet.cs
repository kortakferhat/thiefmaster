using System.Collections.Generic;
using Infrastructure.Managers.PoolManager;
using Gameplay.Projectiles;
using Gameplay.Enemy;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public class HomingBullet : BaseBullet
    {
        [Header("Homing Settings")]
        [SerializeField] private float homingStrength = 2f;
        [SerializeField] private float maxLifetime = 8f;
        [SerializeField] private float retargetInterval = 0.5f;
        [SerializeField] private float targetingRange = 15f;
        
        private Gameplay.Enemy.Enemy currentTarget;
        private float currentLifetime;
        private float lastRetargetTime;
        private bool hasExplicitTarget;

        protected override string GetDefaultPoolKey()
        {
            return PoolKeys.HomingBullet;
        }

        protected override void OnBulletFired(Vector3 target)
        {
            // Reset homing data when bullet is fired
            currentLifetime = 0f;
            lastRetargetTime = 0f;
            hasExplicitTarget = false;
            
            // Try to find initial target
            FindAndSetTarget();
        }

        protected override void OnBulletUpdate()
        {
            currentLifetime += Time.deltaTime;
            
            // Check lifetime limit
            if (currentLifetime >= maxLifetime)
            {
                Despawn();
                return;
            }

            // Periodically retarget to handle target switching or initial targeting
            if (Time.time - lastRetargetTime >= retargetInterval)
            {
                if (currentTarget == null || !IsValidTarget(currentTarget))
                {
                    FindAndSetTarget();
                }
                lastRetargetTime = Time.time;
            }

            // Apply homing behavior if we have a target
            if (currentTarget != null && IsValidTarget(currentTarget))
            {
                ApplyHomingForce();
            }
        }

        private void FindAndSetTarget()
        {
            // Use EnemyUtils to get nearest enemy, then validate it's within our targeting range
            var nearestEnemy = EnemyUtils.GetNearestEnemy(transform.position);
            
            if (nearestEnemy != null)
            {
                float distance = Vector3.Distance(nearestEnemy.transform.position, transform.position);
                if (distance <= targetingRange)
                {
                    currentTarget = nearestEnemy;
                    return;
                }
            }
            
            // If no enemy within range, clear target
            currentTarget = null;
        }

        private bool IsValidTarget(Gameplay.Enemy.Enemy target)
        {
            if (target == null || target.gameObject == null)
                return false;

            // Check if target is still active and within reasonable range
            float distance = Vector3.Distance(target.transform.position, transform.position);
            return distance <= targetingRange * 2f; // Allow some extra range for pursuit
        }

        private void ApplyHomingForce()
        {
            Vector3 targetDirection = (currentTarget.transform.position - transform.position).normalized;
            Vector3 currentDirection = rb.linearVelocity.normalized;
            
            // Calculate desired direction (blend of current and target direction)
            Vector3 desiredDirection = Vector3.Slerp(currentDirection, targetDirection, homingStrength * Time.deltaTime);
            
            // Apply the new velocity
            rb.linearVelocity = desiredDirection * speed;
            
            // Update visual rotation
            rb.transform.rotation = Quaternion.LookRotation(desiredDirection);
            
            // Update direction for base class
            direction = desiredDirection;
        }

        protected override void OnEnemyHit(Collider enemyCollider, Gameplay.Enemy.Enemy enemy)
        {
            // Homing bullets despawn after hitting their target
            Despawn();
        }

        protected override void OnBeforeDespawn()
        {
            // Clear target reference
            currentTarget = null;
            currentLifetime = 0f;
        }

        protected override bool ShouldDespawnOnNonTowerHit(Collider other)
        {
            // Homing bullets should despawn on hitting walls or boundaries
            return true;
        }

        // Add visual debugging for target tracking
        private void OnDrawGizmosSelected()
        {
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
                Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, targetingRange);
        }
    }
} 