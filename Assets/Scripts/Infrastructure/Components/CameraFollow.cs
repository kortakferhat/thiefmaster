using System;
using Gameplay;
using UnityEngine;
using Gameplay.Character;
using Infrastructure.Managers.CameraManager;
using Infrastructure;
using Infrastructure.Events;

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
        
        [Header("Double Tap Control")]
        [SerializeField] private bool enableAutoResume = true;
        [SerializeField] private float autoResumeDelay = 2f;
        
        [Header("Fixed Camera Settings")]
        [SerializeField] private Vector3 fixedCameraPosition = new Vector3(0f, 15f, -10f);
        [SerializeField] private Vector3 fixedCameraRotation = new Vector3(45f, 0f, 0f);
        [SerializeField] private bool useFixedCameraRotation = true;
        
        private Camera _camera;
        private ICameraManager _cameraManager;
        private Vector3 _currentVelocity;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        
        // Camera mode control variables
        private CameraMode _currentMode = CameraMode.Follow;
        private bool _isFollowing = true;
        private Vector3 _stoppedPosition;
        private Quaternion _stoppedRotation;
        private float _autoResumeTimer = 0f;
        
        // Fixed camera variables
        private Vector3 _savedFixedPosition;
        private Quaternion _savedFixedRotation;
        
        // Follow mode restore variables
        private Vector3 _followModePosition;
        private Quaternion _followModeRotation;
        private bool _hasFollowModeData = false;
        
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
            _savedFixedRotation = Quaternion.Euler(fixedCameraRotation);
            
            // Initialize follow mode data with current camera state
            if (_currentMode == CameraMode.Follow)
            {
                _followModePosition = _camera.transform.position;
                _followModeRotation = _camera.transform.rotation;
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
                if (followRotation)
                {
                    _camera.transform.rotation = CalculateTargetRotation();
                }
            }
            else
            {
                _camera.transform.position = fixedCameraPosition;
                if (useFixedCameraRotation)
                {
                    _camera.transform.rotation = Quaternion.Euler(fixedCameraRotation);
                }
            }
        }
        
        private void SubscribeToEvents()
        {
            EventBus.Subscribe<InputEvents.DoubleTapEvent>(OnDoubleTapDetected);
        }
        
        private void OnDoubleTapDetected(InputEvents.DoubleTapEvent doubleTapEvent)
        {
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
                
                // Restore previous follow mode position and rotation if available
                if (_hasFollowModeData)
                {
                    _camera.transform.position = _followModePosition;
                    _camera.transform.rotation = _followModeRotation;
                    Debug.Log("[CameraFollow] Switched to Follow mode. Restored position: " + _followModePosition + ", rotation: " + _followModeRotation.eulerAngles);
                }
                else
                {
                    // Fallback to calculated position if no saved data
                    _camera.transform.position = CalculateTargetPosition();
                    if (followRotation)
                    {
                        _camera.transform.rotation = CalculateTargetRotation();
                    }
                    Debug.Log("[CameraFollow] Switched to Follow mode. Using calculated position.");
                }
            }
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
                _followModeRotation = _camera.transform.rotation;
                _hasFollowModeData = true;
                
                _currentMode = CameraMode.Fixed;
                _isFollowing = false;
                
                // Move camera to fixed position immediately
                _camera.transform.position = fixedCameraPosition;
                if (useFixedCameraRotation)
                {
                    _camera.transform.rotation = Quaternion.Euler(fixedCameraRotation);
                }
                
                Debug.Log("[CameraFollow] Switched to Fixed mode. Saved follow position: " + _followModePosition + ", rotation: " + _followModeRotation.eulerAngles);
            }
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
                _stoppedRotation = _camera.transform.rotation;
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
                if (followRotation)
                {
                    _camera.transform.rotation = CalculateTargetRotation();
                }
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
                _camera.transform.rotation = _stoppedRotation;
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
        /// Sets the fixed camera rotation in euler angles
        /// </summary>
        public void SetFixedCameraRotation(Vector3 rotation)
        {
            fixedCameraRotation = rotation;
            if (_currentMode == CameraMode.Fixed && useFixedCameraRotation)
            {
                _camera.transform.rotation = Quaternion.Euler(rotation);
            }
        }
        
        /// <summary>
        /// Sets the fixed camera rotation using quaternion
        /// </summary>
        public void SetFixedCameraRotation(Quaternion rotation)
        {
            _savedFixedRotation = rotation;
            fixedCameraRotation = rotation.eulerAngles;
            if (_currentMode == CameraMode.Fixed && useFixedCameraRotation)
            {
                _camera.transform.rotation = rotation;
            }
        }
        
        /// <summary>
        /// Sets whether to use fixed camera rotation
        /// </summary>
        public void SetUseFixedCameraRotation(bool useRotation)
        {
            useFixedCameraRotation = useRotation;
            if (_currentMode == CameraMode.Fixed && useRotation)
            {
                _camera.transform.rotation = Quaternion.Euler(fixedCameraRotation);
            }
        }
        
        /// <summary>
        /// Saves current camera position and rotation as fixed camera settings
        /// </summary>
        public void SaveCurrentAsFixed()
        {
            _savedFixedPosition = _camera.transform.position;
            _savedFixedRotation = _camera.transform.rotation;
            fixedCameraPosition = _savedFixedPosition;
            fixedCameraRotation = _savedFixedRotation.eulerAngles;
            
            Debug.Log("[CameraFollow] Current camera position and rotation saved as fixed camera settings.");
        }
        
        /// <summary>
        /// Restores saved fixed camera settings
        /// </summary>
        public void RestoreSavedFixed()
        {
            fixedCameraPosition = _savedFixedPosition;
            fixedCameraRotation = _savedFixedRotation.eulerAngles;
            
            if (_currentMode == CameraMode.Fixed)
            {
                _camera.transform.position = _savedFixedPosition;
                _camera.transform.rotation = _savedFixedRotation;
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
        }
    }
} 