using Gameplay.Enemy;
using Gameplay.Floors.Turret.Weapons;
using Gameplay.Projectiles;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay
{
    public class HybridTurretWeapon : BaseWeapon
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.35f;
    
        [Header("Bullet Type Probabilities")]
        [SerializeField] private float normalBulletChance = 0.4f;
        [SerializeField] private float bouncingBulletChance = 0.3f;
        [SerializeField] private float piercingBulletChance = 0.2f;
        [SerializeField] private float explosiveBouncingBulletChance = 0.1f;

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

            // Determine which bullet type to fire based on probability
            var bulletTypeData = DetermineBulletType();
        
            var bullet = poolManager.Spawn(bulletTypeData.poolKey, firePoint.position, Quaternion.identity).GetComponent<BaseBullet>();
            var offset = (nearestEnemy.transform.position - firePoint.position).normalized * 1;
            bullet.transform.position = firePoint.position + offset;
            bullet.Fire(nearestEnemy.transform.position, bulletTypeData.poolKey);
        
            return true;
        }

        private (string poolKey, System.Type bulletType) DetermineBulletType()
        {
            float randomValue = Random.Range(0f, 1f);
            float cumulativeProbability = 0f;

            // Check normal bullet
            cumulativeProbability += normalBulletChance;
            if (randomValue <= cumulativeProbability)
            {
                return (PoolKeys.Bullet, typeof(Bullet));
            }

            // Check bouncing bullet
            cumulativeProbability += bouncingBulletChance;
            if (randomValue <= cumulativeProbability)
            {
                return (PoolKeys.BouncingBullet, typeof(BouncingBullet));
            }

            // Check piercing bullet
            cumulativeProbability += piercingBulletChance;
            if (randomValue <= cumulativeProbability)
            {
                return (PoolKeys.PiercingBullet, typeof(PiercingBullet));
            }

            // Default to explosive bouncing bullet
            return (PoolKeys.ExplosiveBouncingBullet, typeof(ExplosiveBouncingBullet));
        }
    }
} 