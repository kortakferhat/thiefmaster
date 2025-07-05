using Gameplay;
using Gameplay.Enemy;
using Gameplay.Projectiles;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public abstract class BaseBullet : BaseEntity, IProjectile
    {
        [Header("Base Bullet Settings")]
        [SerializeField] protected Transform visualRoot;
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected float speed = 20f;
        [SerializeField] protected int damage = 1;

        protected IPoolManager poolManager;
        protected IParticleManager particleManager;
        protected Vector3 direction;
        protected string poolKey;

        private void Start()
        {
            Initialize();
        }

        public virtual void Fire(Vector3 target, string bulletPoolKey = null)
        {
            poolKey = bulletPoolKey ?? GetDefaultPoolKey();
            
            direction = (target - rb.position).normalized;
            rb.transform.LookAt(target);
            rb.linearVelocity = direction * speed;
            
            if (poolManager == null)
            {
                poolManager = ServiceLocator.Get<IPoolManager>();
            }

            if (particleManager == null)
            {
                particleManager = ServiceLocator.Get<IParticleManager>();
            }

            OnBulletFired(target);
        }

        protected override void OnEntityUpdate()
        {
            OnBulletUpdate();
        }

        protected override void OnEntityFixedUpdate()
        {
            OnBulletFixedUpdate();
        }

        protected override void OnEntityTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                HandleEnemyHit(other);
            }

            if (!other.CompareTag("Tower"))
            {
                HandleNonTowerHit(other);
            }
        }

        protected override void OnEntityTriggerStay(Collider other)
        {
            OnBulletTriggerStay(other);
        }

        protected override void OnEntityTriggerExit(Collider other)
        {
            OnBulletTriggerExit(other);
        }

        protected virtual void HandleEnemyHit(Collider enemyCollider)
        {
            var offset = -rb.linearVelocity.normalized * 0.65f;
            particleManager.PlayParticle(PoolKeys.HitVFX, enemyCollider.transform.position + offset, Quaternion.identity);
            
            // Deal damage to the enemy
            var enemy = enemyCollider.GetComponent<Gameplay.Enemy.Enemy>();
            enemy.OnHit(damage);

            OnEnemyHit(enemyCollider, enemy);
        }

        protected virtual void HandleNonTowerHit(Collider other)
        {
            if (ShouldDespawnOnNonTowerHit(other))
            {
                Despawn();
            }
        }

        protected virtual void Despawn()
        {
            OnBeforeDespawn();
            poolManager.Despawn(poolKey, gameObject);
        }

        // Abstract/Virtual methods for subclasses to override
        protected abstract string GetDefaultPoolKey();
        protected virtual void OnBulletFired(Vector3 target) { }
        protected virtual void OnBulletUpdate() { }
        protected virtual void OnBulletFixedUpdate() { }
        protected virtual void OnBulletTriggerStay(Collider other) { }
        protected virtual void OnBulletTriggerExit(Collider other) { }
        protected virtual void OnEnemyHit(Collider enemyCollider, Gameplay.Enemy.Enemy enemy) { }
        protected virtual bool ShouldDespawnOnNonTowerHit(Collider other) { return true; }
        protected virtual void OnBeforeDespawn() { }
    }
} 