using Gameplay.Enemy;
using Gameplay.Floors.Turret.Weapons;
using Gameplay.Projectiles;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay
{
    public class PiercingTurretWeapon : BaseWeapon
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.3f; // Slower fire rate for powerful piercing bullets

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
            var nearestEnemy = EnemyUtils.GetNearestEnemy(firePoint.position);

            if (nearestEnemy == null)
            {
                return false;
            }
        
            var bullet = poolManager.Spawn(PoolKeys.PiercingBullet, firePoint.position, Quaternion.identity).GetComponent<PiercingBullet>();
            var offset = (nearestEnemy.transform.position - firePoint.position).normalized * 1;
            bullet.transform.position = firePoint.position + offset;
            bullet.Fire(nearestEnemy.transform.position, PoolKeys.PiercingBullet);
            return true;
        }
    }
} 