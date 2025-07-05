using Gameplay.Enemy;
using Gameplay.Floors.Turret.Weapons;
using Gameplay.Projectiles;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay
{
    public class HomingTurretWeapon : BaseWeapon
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.6f; // Higher fire rate since homing bullets are very effective

        private IPoolManager poolManager;
        private IGameManager gameManager;
        private float nextFireTime;
    
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
        
            if (Time.time >= nextFireTime)
            {
                var fired = Fire();
            
                if (fired)
                {
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }
    
        bool Fire()
        {
            // For homing bullets, we don't necessarily need an immediate target
            // The bullet will find its own target, but we still check for enemies in range
            var nearestEnemy = EnemyUtils.GetNearestEnemy(firePoint.position);

            if (nearestEnemy == null)
            {
                return false;
            }
        
            var bullet = poolManager.Spawn(PoolKeys.HomingBullet, firePoint.position, Quaternion.identity).GetComponent<HomingBullet>();
            var offset = (nearestEnemy.transform.position - firePoint.position).normalized * 1;
            bullet.transform.position = firePoint.position + offset;
            // Fire towards general direction, bullet will handle homing
            bullet.Fire(nearestEnemy.transform.position, PoolKeys.HomingBullet);
            return true;
        }
    }
} 