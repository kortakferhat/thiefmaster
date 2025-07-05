using Gameplay.Floors.Turret.Weapons;
using Gameplay.Projectiles;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay
{
    public class WaveWeapon : BaseWeapon
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.3f; // Slower fire rate for waves

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
            // For waves, we don't need to target a specific enemy
            // The wave will expand and hit all enemies in its radius
            var wave = poolManager.Spawn(PoolKeys.Wave, firePoint.position, Quaternion.identity).GetComponent<Wave>();
            wave.Fire(firePoint.position);
            return true;
        }
    }
} 