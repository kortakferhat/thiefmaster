using Gameplay.Enemy;
using Gameplay.Floors.Turret.Weapons;
using Gameplay.Projectiles;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay
{
    public class TurretWeapon : BaseWeapon
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = .5f;
        [SerializeField] private float rotationSpeed = 10f; // How quickly the turret rotates to face the enemy

        private IPoolManager poolManager;
        private IGameManager gameManager;
        private float nextFireTime;
        private Transform targetEnemy; // Track the current target
    
        public override void Initialize()
        {
            base.Initialize();
        
            poolManager = ServiceLocator.Get<IPoolManager>();
            gameManager = ServiceLocator.Get<IGameManager>();
        }
    
        protected override void OnEntityUpdate()
        {
            if (gameManager.State != GameState.Game)
            {
                return;
            }

            // Find and track nearest enemy
            targetEnemy = EnemyUtils.GetNearestEnemy(firePoint.position)?.transform;
            
            // Rotate towards enemy if we have a target
            if (targetEnemy != null)
            {
                RotateTowardsTarget();
            }
        
            if (Time.time >= nextFireTime)
            {
                var fired = Fire();
            
                if (fired)
                {
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }
        
        private void RotateTowardsTarget()
        {
            if (targetEnemy == null) return;
            
            // Calculate direction to enemy
            Vector3 direction = targetEnemy.position - transform.position;
            direction.y = 0; // Keep rotation on the horizontal plane only
            
            // Only rotate if we have a direction
            if (direction != Vector3.zero)
            {
                // Create the rotation we need to perform
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                // Smoothly rotate to that direction
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    
        bool Fire()
        {
            if (targetEnemy == null)
            {
                return false;
            }
            
            // Check if we're facing the target enough to fire
            Vector3 directionToTarget = (targetEnemy.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            
            // Only fire if we're mostly facing the target (dot product > 0.9 means within ~25 degrees)
            if (dotProduct > 0.9f)
            {
                var bullet = poolManager.Spawn(PoolKeys.Bullet, firePoint.position, Quaternion.identity).GetComponent<Bullet>();
                var offset = (targetEnemy.position - firePoint.position).normalized * .25f;
                bullet.transform.position = firePoint.position + offset;
                bullet.Fire(targetEnemy.position);
                return true;
            }
            
            return false;
        }
    }
}