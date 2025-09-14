using System;
using Gameplay;
using UnityEngine;
using Gameplay.Character;
using Infrastructure.Managers.CameraManager;
using Infrastructure;
using Infrastructure.Events;
using DG.Tweening;

namespace Infrastructure.Components
{
    public class CameraFollow : BaseEntity
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 0f); // Z ekseni kullanılmıyor
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float damping = 0.1f;
        [SerializeField] private bool smoothFollow = true;
        
        [Header("Camera Settings")]
        [SerializeField] private CameraType cameraType = CameraType.Main;
        
        [Header("Double Tap Control")]
        [SerializeField] private bool enableAutoResume = true;
        [SerializeField] private float autoResumeDelay = 2f;
        
        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private Ease transitionEase = Ease.InOutQuad;
        
        [Header("Fixed Camera Settings")]
        [SerializeField] private Vector3 fixedCameraPosition = new Vector3(0f, 0f, 0f); // Z ekseni kullanılmıyor
        
        private Camera _camera;
        private ICameraManager _cameraManager;
        private Vector3 _currentVelocity;
        private Vector3 _targetPosition;
        
        // Camera mode control variables
        private CameraMode _currentMode = CameraMode.Follow;
        private bool _isFollowing = true;
        private Vector3 _stoppedPosition;
        private float _autoResumeTimer = 0f;
        
        // Fixed camera variables
        private Vector3 _savedFixedPosition;
        
        // Follow mode restore variables
        private Vector3 _followModePosition;
        private bool _hasFollowModeData = false;
        
        // Transition variables
        private bool _isTransitioning = false;
        private Tween _positionTween;
        
        public enum CameraType
        {
            Main,
            Top
        }
        
        public enum CameraMode
        {
            Follow,
            Fixed
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
        
        public bool IsFollowing => _isFollowing;
        
        public CameraMode CurrentMode => _currentMode;
        
        protected override void OnEntityUpdate()
        {
            switch (_currentMode)
            {
                case CameraMode.Follow:
                    if (_isFollowing)
                    {
                        UpdateCameraFollow();
                    }
                    else if (enableAutoResume)
                    {
                        UpdateAutoResume();
                    }
                    break;
                    
                case CameraMode.Fixed:
                    // Fixed camera mode - no movement needed
                    break;
            }
        }
        
        public override void Initialize()
        {
            base.Initialize();
            ResolveCameraManager();
            SetupCamera();
            SubscribeToEvents();
            
            // Save initial fixed camera settings
            _savedFixedPosition = fixedCameraPosition;
            
            // Initialize follow mode data with current camera state
            if (_currentMode == CameraMode.Follow)
            {
                _followModePosition = _camera.transform.position;
                _hasFollowModeData = true;
            }
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
            
            // Set initial position based on current mode
            if (_currentMode == CameraMode.Follow)
            {
                _camera.transform.position = CalculateTargetPosition();
                // 2D'de kamera rotasyonu sabit kalır (orthographic için)
                _camera.transform.rotation = Quaternion.identity;
            }
            else
            {
                _camera.transform.position = fixedCameraPosition;
                // 2D'de kamera rotasyonu sabit kalır (orthographic için)
                _camera.transform.rotation = Quaternion.identity;
            }
        }
        
        private void SubscribeToEvents()
        {
            EventBus.Subscribe<InputEvents.DoubleTapEvent>(OnDoubleTapDetected);
        }
        
        private void OnDoubleTapDetected(InputEvents.DoubleTapEvent doubleTapEvent)
        {
            // If transitioning, ignore the input
            if (_isTransitioning)
            {
                Debug.Log("[CameraFollow] Ignoring double tap during transition.");
                return;
            }
            
            // Double tap switches between camera modes
            ToggleCameraMode();
        }
        
        private void UpdateAutoResume()
        {
            if (enableAutoResume && !_isFollowing && _currentMode == CameraMode.Follow)
            {
                _autoResumeTimer += Time.deltaTime;
                if (_autoResumeTimer >= autoResumeDelay)
                {
                    ResumeFollow();
                }
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
            
            // 2D'de kamera rotasyonu sabit kalır (orthographic için)
            _camera.transform.rotation = Quaternion.identity;
        }
        
        private Vector3 CalculateTargetPosition()
        {
            Vector3 targetPos = target.position + offset;
            return targetPos;
        }
        
        /// <summary>
        /// Switches camera to follow mode
        /// </summary>
        public void SwitchToFollowMode()
        {
            if (_currentMode == CameraMode.Fixed)
            {
                _currentMode = CameraMode.Follow;
                _isFollowing = true;
                _autoResumeTimer = 0f;
                
                // Start transition to follow mode
                StartTransitionToFollowMode();
            }
        }
        
        private void StartTransitionToFollowMode()
        {
            if (_isTransitioning) return;
            
            _isTransitioning = true;
            Vector3 targetPosition;
            
            // Determine target position
            if (_hasFollowModeData)
            {
                targetPosition = _followModePosition;
                Debug.Log("[CameraFollow] Transitioning to Follow mode. Target position: " + targetPosition);
            }
            else
            {
                targetPosition = CalculateTargetPosition();
                Debug.Log("[CameraFollow] Transitioning to Follow mode. Using calculated position.");
            }
            
            // Animate position transition
            _positionTween = _camera.transform.DOMove(targetPosition, transitionDuration)
                .SetEase(transitionEase)
                .OnComplete(() => OnPositionTransitionComplete());
                
            // 2D'de kamera rotasyonu sabit kalır
            _camera.transform.rotation = Quaternion.identity;
        }
        
        /// <summary>
        /// Switches camera to fixed mode
        /// </summary>
        public void SwitchToFixedMode()
        {
            if (_currentMode == CameraMode.Follow)
            {
                // Save current follow mode state before switching
                _followModePosition = _camera.transform.position;
                _hasFollowModeData = true;
                
                _currentMode = CameraMode.Fixed;
                _isFollowing = false;
                
                // Start transition to fixed mode
                StartTransitionToFixedMode();
            }
        }
        
        private void StartTransitionToFixedMode()
        {
            if (_isTransitioning) return;
            
            _isTransitioning = true;
            Vector3 targetPosition = fixedCameraPosition;
            
            Debug.Log("[CameraFollow] Transitioning to Fixed mode. Target position: " + targetPosition);
            
            // Animate position transition
            _positionTween = _camera.transform.DOMove(targetPosition, transitionDuration)
                .SetEase(transitionEase)
                .OnComplete(() => OnPositionTransitionComplete());
                
            // 2D'de kamera rotasyonu sabit kalır
            _camera.transform.rotation = Quaternion.identity;
        }
        
        private void OnPositionTransitionComplete()
        {
            // Position transition completed
            Debug.Log("[CameraFollow] Position transition completed.");
            
            // Transition is complete, reset transitioning state
            _isTransitioning = false;
            Debug.Log("[CameraFollow] All transitions completed. Camera mode switch finished.");
        }
        
        /// <summary>
        /// Switches between follow and fixed modes
        /// </summary>
        public void ToggleCameraMode()
        {
            if (_currentMode == CameraMode.Follow)
            {
                SwitchToFixedMode();
            }
            else if (_currentMode == CameraMode.Fixed)
            {
                SwitchToFollowMode();
            }
        }
        
        /// <summary>
        /// Stops the camera from following the target and maintains current position
        /// </summary>
        public void StopFollow()
        {
            if (_isFollowing && _currentMode == CameraMode.Follow)
            {
                _isFollowing = false;
                _stoppedPosition = _camera.transform.position;
                _autoResumeTimer = 0f;
                
                Debug.Log("[CameraFollow] Camera follow stopped. Camera will maintain current position.");
            }
        }
        
        /// <summary>
        /// Resumes following the target from the current camera position
        /// </summary>
        public void ResumeFollow()
        {
            if (!_isFollowing && _currentMode == CameraMode.Follow)
            {
                _isFollowing = true;
                _autoResumeTimer = 0f;
                Debug.Log("[CameraFollow] Camera follow resumed.");
            }
        }
        
        /// <summary>
        /// Resumes following the target and immediately moves to the target position
        /// </summary>
        public void ResumeFollowAndSnap()
        {
            if (!_isFollowing && _currentMode == CameraMode.Follow)
            {
                _isFollowing = true;
                _autoResumeTimer = 0f;
                _camera.transform.position = CalculateTargetPosition();
                // 2D'de kamera rotasyonu sabit kalır
                _camera.transform.rotation = Quaternion.identity;
                Debug.Log("[CameraFollow] Camera follow resumed and snapped to target position.");
            }
        }
        
        /// <summary>
        /// Returns the camera to the position it was at when StopFollow was called
        /// </summary>
        public void ReturnToStoppedPosition()
        {
            if (!_isFollowing && _currentMode == CameraMode.Follow)
            {
                _camera.transform.position = _stoppedPosition;
                // 2D'de kamera rotasyonu sabit kalır
                _camera.transform.rotation = Quaternion.identity;
                Debug.Log("[CameraFollow] Camera returned to stopped position.");
            }
        }
        
        /// <summary>
        /// Sets the fixed camera position
        /// </summary>
        public void SetFixedCameraPosition(Vector3 position)
        {
            fixedCameraPosition = position;
            if (_currentMode == CameraMode.Fixed)
            {
                _camera.transform.position = position;
            }
        }
        
        
        /// <summary>
        /// Saves current camera position as fixed camera settings
        /// </summary>
        public void SaveCurrentAsFixed()
        {
            _savedFixedPosition = _camera.transform.position;
            fixedCameraPosition = _savedFixedPosition;
            
            Debug.Log("[CameraFollow] Current camera position saved as fixed camera settings.");
        }
        
        /// <summary>
        /// Restores saved fixed camera settings
        /// </summary>
        public void RestoreSavedFixed()
        {
            fixedCameraPosition = _savedFixedPosition;
            
            if (_currentMode == CameraMode.Fixed)
            {
                _camera.transform.position = _savedFixedPosition;
                _camera.transform.rotation = Quaternion.identity;
            }
            
            Debug.Log("[CameraFollow] Saved fixed camera settings restored.");
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
        
        
        // Method to set offset based on camera angle
        public void SetOffsetByAngle(float height, float distance, float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            offset = new Vector3(
                -Mathf.Sin(radians) * distance,
                height,
                0f // Z ekseni kullanılmıyor
            );
        }
        
        public void SetAutoResume(bool enabled)
        {
            enableAutoResume = enabled;
        }
        
        public void SetAutoResumeDelay(float delay)
        {
            autoResumeDelay = Mathf.Max(0.1f, delay);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<InputEvents.DoubleTapEvent>(OnDoubleTapDetected);
            
            // Kill any active tweens
            if (_positionTween != null && _positionTween.IsActive())
            {
                _positionTween.Kill();
            }
        }
    }
} 