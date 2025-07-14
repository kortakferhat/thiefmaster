using System;
using DG.Tweening;
using Gameplay.Rewards;
using Infrastructure;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using TowerClicker.Infrastructure.Managers.CameraManager;
using UnityEngine;

namespace Gameplay.Enemy{
    public class Enemy : BaseEntity
    {
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private EnemyConfigSO enemyConfig;

        private readonly float stopDistance = 1.5f;
        private readonly float pushbackDistanceOnHit = .75f;
        private readonly int coinRewardAmount = 1; // TODO: Make this configurable
        private float lastHitTime;
        private float movementSlowdownFactor;

        private IPoolManager poolManager;
        private IParticleManager particleManager;
        private IEnemyManager enemyManager;
        private ICameraManager cameraManager;

        private void Start()
        {
            _gameManager.OnStateChanged += OnGameStateChanged;
            
            poolManager = ServiceLocator.Get<IPoolManager>();
            particleManager = ServiceLocator.Get<IParticleManager>();
            enemyManager = ServiceLocator.Get<IEnemyManager>();
            cameraManager = ServiceLocator.Get<ICameraManager>();

            hudCanvas.worldCamera = cameraManager.GetMainCamera();
        }

        public override void Initialize()
        {
            base.Initialize();
            
            var healthBarData = new HealthBarData()
            {
                MaxHealth = enemyConfig.health,
                CurrentHealth = enemyConfig.health
            };
            healthBar.Initialize(healthBarData, transform, Vector3.up * 1.5f);

            PlaySpawnAnimations();
        }

        private void PlaySpawnAnimations()
        {
            healthBar.transform.localScale = Vector3.zero;
            
            transform.localScale = Vector3.one * .25f;
            transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
            healthBar.transform.DOScale(Vector3.one, .225f).SetEase(Ease.OutBack).SetDelay(.125f);
        }
        
        protected override void OnEntityFixedUpdate()
        {
            MoveToCenter();
        }
        
        protected override void OnEntityTriggerStay(Collider other)
        {
        }
        
        private void MoveToCenter()
        {
            var position = rb.position;
            position.y = 0;
            var distanceToOrigin = Vector3.Distance(position, Vector3.zero);
            
            if (distanceToOrigin > stopDistance)
            {
                var direction = (Vector3.zero - position).normalized;
                var movement = direction * (enemyConfig.speed * movementSlowdownFactor * Time.fixedDeltaTime);
                rb.transform.position += movement;
                
                rb.transform.rotation = Quaternion.LookRotation(direction);
            }

            if (movementSlowdownFactor < 1)
            {
                movementSlowdownFactor += Time.fixedDeltaTime;
                
                if (movementSlowdownFactor >= 1)
                {
                    movementSlowdownFactor = 1;
                }
            }
        }

        public void OnHit(int damage)
        {
            healthBar.DecreaseHealth(damage);
            movementSlowdownFactor = 0.1f;
            
            var position = rb.position;
            position.y = 0;
            var direction = (position - Vector3.zero).normalized;
            var pushback = direction * (pushbackDistanceOnHit);
            rb.transform.position += pushback;
            
            if (healthBar.HealthBarData.CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            enemyManager.DespawnEnemy(gameObject);
            particleManager.PlayParticle(PoolKeys.EnemyExplosion, transform.position, Quaternion.identity);

            var offset = Vector3.up * .5f;
            var coin = poolManager.Spawn(PoolKeys.Coin, transform.position + offset, Quaternion.identity);
            coin.GetComponent<Coin>().MoveToCenter();
        }
        
        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.GameOver)
            {
                Die();
            }
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnStateChanged -= OnGameStateChanged;
            }
        }
    }
}