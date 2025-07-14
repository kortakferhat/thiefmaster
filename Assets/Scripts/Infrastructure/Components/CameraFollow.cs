using System;
using Gameplay;
using UnityEngine;
using TowerClicker.Infrastructure;
using TowerClicker.Infrastructure.Managers.CameraManager;
using Gameplay.Character;

namespace Infrastructure.Components
{
    public class CameraFollow : BaseEntity
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -5f);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float damping = 0.1f;
        [SerializeField] private bool followRotation = true;
        [SerializeField] private bool smoothFollow = true;
        
        [Header("Camera Settings")]
        [SerializeField] private CameraType cameraType = CameraType.Main;
        
        private Camera _camera;
        private ICameraManager _cameraManager;
        private Vector3 _currentVelocity;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        
        public enum CameraType
        {
            Main,
            Top
        }
        
        public Transform Target
        {
            get => target;
            set => target = value;
        }
        
        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
        }
        
        public float FollowSpeed
        {
            get => followSpeed;
            set => followSpeed = value;
        }
        
        public float Damping
        {
            get => damping;
            set => damping = value;
        }
        
        protected override void OnEntityUpdate()
        {
            UpdateCameraFollow();
        }
        
        public override void Initialize()
        {
            base.Initialize();
            ResolveCameraManager();
            SetupCamera();
        }
        
        private void ResolveCameraManager()
        {
            _cameraManager ??= ServiceLocator.Get<ICameraManager>();
        }
        
        private void SetupCamera()
        {
            switch (cameraType)
            {
                case CameraType.Main:
                    _camera = _cameraManager.GetMainCamera();
                    break;
                case CameraType.Top:
                    _camera = _cameraManager.GetTopCamera();
                    break;
            }
            
            // Set initial position
            _camera.transform.position = CalculateTargetPosition();
            if (followRotation)
            {
                _camera.transform.rotation = CalculateTargetRotation();
            }
        }
        
        private void UpdateCameraFollow()
        {
            _targetPosition = CalculateTargetPosition();
            
            if (smoothFollow)
            {
                _camera.transform.position = Vector3.SmoothDamp(
                    _camera.transform.position, 
                    _targetPosition, 
                    ref _currentVelocity, 
                    damping
                );
            }
            else
            {
                _camera.transform.position = Vector3.Lerp(
                    _camera.transform.position, 
                    _targetPosition, 
                    followSpeed * Time.deltaTime
                );
            }
            
            if (followRotation)
            {
                _targetRotation = CalculateTargetRotation();
                _camera.transform.rotation = Quaternion.Slerp(
                    _camera.transform.rotation, 
                    _targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        
        private Vector3 CalculateTargetPosition()
        {
            Vector3 targetPos = target.position + offset;
            return targetPos;
        }
        
        private Quaternion CalculateTargetRotation()
        {
            Vector3 direction = target.position - _camera.transform.position;
            if (direction != Vector3.zero)
            {
                return Quaternion.LookRotation(direction);
            }
            
            return _camera.transform.rotation;
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
        
        public void SetFollowSpeed(float newSpeed)
        {
            followSpeed = Mathf.Max(0.1f, newSpeed);
        }
        
        public void SetRotationSpeed(float newSpeed)
        {
            rotationSpeed = Mathf.Max(0.1f, newSpeed);
        }
        
        public void SetDamping(float newDamping)
        {
            damping = Mathf.Clamp01(newDamping);
        }
        
        public void SetCameraType(CameraType type)
        {
            cameraType = type;
            SetupCamera();
        }
        
        public void SetSmoothFollow(bool smooth)
        {
            smoothFollow = smooth;
        }
        
        public void SetFollowRotation(bool follow)
        {
            followRotation = follow;
        }
        
        // Method to set offset based on camera angle
        public void SetOffsetByAngle(float height, float distance, float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            offset = new Vector3(
                -Mathf.Sin(radians) * distance,
                height,
                -Mathf.Cos(radians) * distance
            );
        }
    }
} 