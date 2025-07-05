using System;
using TowerClicker.Infrastructure;
using TowerClicker.Infrastructure.Managers.CameraManager;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure
{
    public class HealthBarData
    {
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
    }

    public class HealthBar : MonoBehaviour
    {
        public HealthBarData HealthBarData { get; private set; }
        
        private Transform _targetTransform;
        private Camera _mainCamera;
        private Vector3 _followOffset;
        
        [SerializeField] private Image _barImage;
        private bool _isFixedPosition;

        public void Initialize(HealthBarData healthBarData)
        {
            HealthBarData = healthBarData;
            _isFixedPosition = true;
        }
        
        public void Initialize(HealthBarData healthBarData, Transform targetTransform, Vector3 followOffset)
        {
            HealthBarData = healthBarData;
            _targetTransform = targetTransform;
            _followOffset = followOffset;
            
            _mainCamera = ServiceLocator.Get<ICameraManager>().GetMainCamera();
            
            UpdateHealthBar();
        }

        private void Update()
        {
            UpdatePosition();
        }
        
        private void UpdatePosition()
        {
            if (_isFixedPosition)
            {
                return;
            }
            
            if (_targetTransform == null)
            {
                return;
            }
            
            transform.rotation = _mainCamera.transform.rotation;
        }

        private void UpdateHealthBar()
        {
            if (HealthBarData.MaxHealth <= 0)
            {
                Debug.LogWarning("Max health is zero or negative. Cannot update health bar.");
                return;
            }

            float healthPercentage = HealthBarData.CurrentHealth / HealthBarData.MaxHealth;
            _barImage.fillAmount = Mathf.Clamp01(healthPercentage);
        }

        public void DecreaseHealth(int damage)
        {
            HealthBarData.CurrentHealth -= damage;
            UpdateHealthBar();
        }
        
        public void IncreaseHealth(int amount)
        {
            HealthBarData.CurrentHealth += amount;
            UpdateHealthBar();
        }
    }
}

