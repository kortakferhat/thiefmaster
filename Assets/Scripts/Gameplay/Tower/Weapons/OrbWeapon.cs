using System.Collections.Generic;
using Gameplay.Floors.Turret.Weapons;
using Gameplay.Projectiles;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay
{
    public class OrbWeapon : BaseWeapon
    {
        [SerializeField] private Transform centerPoint;
        [SerializeField] private int orbCount = 3;
        [SerializeField] private float orbRadius = 2f;
        [SerializeField] private float rotationSpeed = 90f; // degrees per second
        [SerializeField] private int orbDamage = 1;

        private IPoolManager _poolManager;
        private readonly List<Orb> _activeOrbs = new List<Orb>();
    
        public override void Initialize()
        {
            base.Initialize();
        
            _poolManager = ServiceLocator.Get<IPoolManager>();
            SpawnOrbs();
        }
    
        private void SpawnOrbs()
        {
            // Clear any existing orbs
            DespawnAllOrbs();
        
            // Calculate angle between orbs for even distribution
            float angleStep = 360f / orbCount;
        
            for (int i = 0; i < orbCount; i++)
            {
                var orbGameObject = _poolManager.Spawn(PoolKeys.Orb, centerPoint.position, Quaternion.identity);
                var orb = orbGameObject.GetComponent<Orb>();
            
                if (orb != null)
                {
                    float startAngle = i * angleStep;
                    orb.Initialize(centerPoint.position, orbRadius, rotationSpeed, orbDamage, startAngle);
                    _activeOrbs.Add(orb);
                }
            }
        }
    
        protected override void OnEntityUpdate()
        {
            if (_gameManager.State != GameState.Game)
            {
                return;
            }
        
            // The orbs will update their positions automatically in their own Update method
            // No need to constantly reinitialize them unless the center position changes significantly
        
            // Update center position for all orbs in case the tower moves
            foreach (var orb in _activeOrbs)
            {
                if (orb != null)
                {
                    orb.UpdateCenterPosition(centerPoint.position);
                }
            }
        }
    
        private void DespawnAllOrbs()
        {
            foreach (var orb in _activeOrbs)
            {
                if (orb != null)
                {
                    orb.Despawn();
                }
            }
            _activeOrbs.Clear();
        }
    
        private void OnDestroy()
        {
            DespawnAllOrbs();
        }
    
        private void OnDisable()
        {
            DespawnAllOrbs();
        }
    }
} 