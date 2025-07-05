using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public class Orb : BaseEntity, IProjectile
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SphereCollider sphereCollider;
        
        private IPoolManager _poolManager;
        private IParticleManager _particleManager;
        
        private Vector3 _centerPosition;
        private float _radius = 2f;
        private float _rotationSpeed = 90f; // degrees per second
        private int _damage = 1;
        private float _currentAngle;
        private readonly float _orbitHeight; // Height above the floor
        private bool _isInitialized = false;
        
        private void Start()
        {
            Initialize();
        }
        
        public void Initialize(Vector3 center, float orbRadius, float speed, int orbDamage, float startAngle)
        {
            _centerPosition = center;
            _radius = orbRadius;
            _rotationSpeed = speed;
            _damage = orbDamage;
            _currentAngle = startAngle;
            _isInitialized = true;
            
            if (_poolManager == null)
            {
                _poolManager = ServiceLocator.Get<IPoolManager>();
            }

            if (_particleManager == null)
            {
                _particleManager = ServiceLocator.Get<IParticleManager>();
            }
            
            UpdatePosition();
        }
        
        protected override void OnEntityUpdate()
        {
            if (!_isInitialized) return;
            
            // Rotate around the center
            _currentAngle += _rotationSpeed * Time.deltaTime;
            if (_currentAngle >= 360f)
            {
                _currentAngle -= 360f;
            }
            
            UpdatePosition();
        }
        
        private void UpdatePosition()
        {
            // Calculate position in a circle around the center
            float radians = _currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * _radius,
                _orbitHeight,
                Mathf.Sin(radians) * _radius
            );
            
            var pos = _centerPosition + offset;
            pos.y = 0;
            transform.position = pos;
        }
        
        protected override void OnEntityFixedUpdate()
        {
        }
        
        protected override void OnEntityTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                // Play hit effect
                _particleManager.PlayParticle(PoolKeys.HitVFX, other.transform.position, Quaternion.identity);
                
                // Deal damage to the enemy
                var enemy = other.GetComponent<Gameplay.Enemy.Enemy>();
                enemy.OnHit(_damage);
            }
        }
        
        protected override void OnEntityTriggerStay(Collider other)
        {
        }
        
        protected override void OnEntityTriggerExit(Collider other)
        {
        }
        
        public void UpdateCenterPosition(Vector3 newCenter)
        {
            _centerPosition = newCenter;
        }
        
        public void Despawn()
        {
            _isInitialized = false;
            _poolManager.Despawn(PoolKeys.Orb, gameObject);
        }
        
        private void OnDisable()
        {
            _isInitialized = false;
        }
    }
}