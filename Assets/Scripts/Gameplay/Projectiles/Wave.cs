using System.Collections.Generic;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public class Wave : BaseEntity, IProjectile
    {
        [SerializeField] private SphereCollider sphereCollider;
        
        private IPoolManager poolManager;
        private IParticleManager particleManager;
        
        private readonly float _expansionSpeed = 7f;
        private readonly float maxRadius = 14f;
        private float _currentRadius = 1f;
        private readonly int _damage = 1;
        private readonly float _lifetime = 10f;
        private float _currentLifetime;
        
        // Track enemies that have already been damaged to prevent multiple hits
        private HashSet<Gameplay.Enemy.Enemy> damagedEnemies = new ();

        private void Start()
        {
            Initialize();
        }

        public void Fire(Vector3 position)
        {
            transform.position = position;
            _currentRadius = 0.5f;
            _currentLifetime = 0f;
            damagedEnemies.Clear();
            
            // Initialize collider
            sphereCollider.transform.localScale = Vector3.one * _currentRadius;
            
            poolManager = ServiceLocator.Get<IPoolManager>();
            particleManager = ServiceLocator.Get<IParticleManager>();
        }

        protected override void OnEntityUpdate()
        {
            // Update lifetime
            _currentLifetime += Time.deltaTime;
            
            // Check if wave should despawn
            if (_currentLifetime >= _lifetime || _currentRadius >= maxRadius)
            {
                Despawn();
                return;
            }
            
            // Expand the wave
            _currentRadius += _expansionSpeed * Time.deltaTime;
            
            // Update collider radius
            sphereCollider.transform.localScale = Vector3.one * _currentRadius;
        }

        protected override void OnEntityFixedUpdate()
        {
        }

        protected override void OnEntityTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<Gameplay.Enemy.Enemy>();
                
                // Check if this enemy has already been damaged by this wave
                if (enemy && damagedEnemies.Add(enemy))
                {
                    // Play hit effect
                    var hitPosition = other.transform.position;
                    particleManager.PlayParticle(PoolKeys.HitVFX, hitPosition, Quaternion.identity);
                    
                    // Deal damage to the enemy
                    enemy.OnHit(_damage);
                }
            }
        }

        protected override void OnEntityTriggerStay(Collider other)
        {
            // Wave continues to expand, no additional damage on stay
        }

        protected override void OnEntityTriggerExit(Collider other)
        {
        }

        private void Despawn()
        {
            // Clear damaged enemies list for next use
            damagedEnemies.Clear();
            
            // Despawn the wave
            poolManager.Despawn(PoolKeys.Wave, gameObject);
        }
    }
}