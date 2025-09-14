using UnityEngine;
using DG.Tweening;

namespace Gameplay.Character
{
    /// <summary>
    /// Component responsible for visual effects on character sprites
    /// Handles breathing animations, state transitions, and other sprite effects
    /// </summary>
    public class CharacterSpriteEffects : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("State Effects")]
        [SerializeField] private float walkScaleUp = 1.3f;
        [SerializeField] private float walkScaleDown = 0.7f;
        [SerializeField] private float walkScaleFinal = 1.1f;
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color walkColor = Color.white;
        [SerializeField] private Color deadColor = Color.gray;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private Tween _walkTween;
        private Vector3 _originalScale;
        private Color _originalColor;
        private CharacterState _currentState = CharacterState.Idle;
        private bool _isInitialized = false;
        private float _movementDuration = 0.5f;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            _isInitialized = true;
        }
        
        /// <summary>
        /// Initialize sprite effects with external parameters
        /// </summary>
        /// <param name="movementDuration">Duration of character movement</param>
        public void Initialize(float movementDuration)
        {
            _movementDuration = movementDuration;
            
            if (showDebugLogs)
                Debug.Log($"[SpriteEffects] Initialized with movement duration: {_movementDuration}");
        }
        
        /// <summary>
        /// Set character facing direction based on movement direction
        /// </summary>
        /// <param name="direction">Movement direction</param>
        public void SetFacingDirection(Vector2Int direction)
        {
            if (direction.x != 0) // Only handle left-right movement
            {
                bool facingRight = direction.x > 0;
                spriteRenderer.flipX = !facingRight;
                
                if (showDebugLogs)
                    Debug.Log($"[SpriteEffects] Facing direction set to: {(facingRight ? "Right" : "Left")}");
            }
        }
        
        private void InitializeComponents()
        {
            // Store original values
            _originalScale = transform.localScale;
            _originalColor = spriteRenderer.color;
            
            if (showDebugLogs)
                Debug.Log($"[SpriteEffects] Initialized with original scale: {_originalScale}");
        }
        
        /// <summary>
        /// Handle character state changes
        /// </summary>
        /// <param name="newState">New character state</param>
        public void OnCharacterStateChanged(CharacterState newState)
        {
            if (!_isInitialized) return;
            
            if (_currentState == newState) return;
            
            var previousState = _currentState;
            _currentState = newState;
            
            if (showDebugLogs)
                Debug.Log($"[SpriteEffects] State changed from {previousState} to {newState}");
            
            // Handle state-specific effects
            switch (newState)
            {
                case CharacterState.Idle:
                    OnIdleState();
                    break;
                    
                case CharacterState.Walk:
                    OnWalkState();
                    break;
                    
                case CharacterState.Dead:
                    OnDeadState();
                    break;
                    
                case CharacterState.Win:
                    OnWinState();
                    break;
                    
                case CharacterState.Attacking:
                    OnAttackingState();
                    break;
                    
                case CharacterState.Defending:
                    OnDefendingState();
                    break;
            }
        }
        
        private void OnIdleState()
        {
            StopWalkSquashStretch();
            SetSpriteColor(idleColor);
        }
        
        private void OnWalkState()
        {
            StartWalkSquashStretch();
            SetSpriteColor(walkColor);
        }
        
        private void OnDeadState()
        {
            StopAllAnimations();
            SetSpriteColor(deadColor);
            // Could add death effect here (fade out, shake, etc.)
        }
        
        private void OnWinState()
        {
            StopAllAnimations();
            // Could add win effect here (sparkle, glow, etc.)
        }
        
        private void OnAttackingState()
        {
            // Could add attack effect here
        }
        
        private void OnDefendingState()
        {
            // Could add defend effect here
        }
        
        /// <summary>
        /// Start walk animation - quick scale up, scale down, then bounce back
        /// </summary>
        private void StartWalkSquashStretch()
        {
            if (_walkTween != null && _walkTween.IsActive())
            {
                _walkTween.Kill();
            }
            
            // Use initialized movement duration
            float movementDuration = _movementDuration;
            
            _walkTween = DOTween.Sequence()
                // Quick scale up (10% of movement)
                .Append(transform.DOScale(_originalScale * walkScaleUp, movementDuration * 0.1f)
                    .SetEase(Ease.OutQuad))
                // Scale down to compressed state (60% of movement)
                .Append(transform.DOScale(_originalScale * walkScaleDown, movementDuration * 0.6f)
                    .SetEase(Ease.InQuad))
                // Bounce back to slightly larger than original (30% of movement)
                .Append(transform.DOScale(_originalScale * walkScaleFinal, movementDuration * 0.3f)
                    .SetEase(Ease.OutBack))
                .SetAutoKill(true);
        }
        
        /// <summary>
        /// Stop walk squash-stretch animation
        /// </summary>
        private void StopWalkSquashStretch()
        {
            if (_walkTween != null && _walkTween.IsActive())
            {
                _walkTween.Kill();
                _walkTween = null;
            }
            
            // Reset scale to original
            transform.localScale = _originalScale;
        }
        
        /// <summary>
        /// Stop all animations and reset to original state
        /// </summary>
        private void StopAllAnimations()
        {
            StopWalkSquashStretch();
        }
        
        /// <summary>
        /// Set sprite color
        /// </summary>
        /// <param name="color">Target color</param>
        private void SetSpriteColor(Color color)
        {
            spriteRenderer.color = color;
        }
        
        /// <summary>
        /// Reset sprite to original state
        /// </summary>
        public void ResetToOriginalState()
        {
            StopAllAnimations();
            transform.localScale = _originalScale;
            SetSpriteColor(_originalColor);
            _currentState = CharacterState.Idle;
            
            if (showDebugLogs)
                Debug.Log($"[SpriteEffects] Reset to original state");
        }
        
        private void OnDestroy()
        {
            // Clean up all tweens
            StopAllAnimations();
            
            // Kill any remaining tweens on this transform
            transform.DOKill();
        }
        
        private void OnDisable()
        {
            if (_walkTween != null && _walkTween.IsActive())
            {
                _walkTween.Pause();
            }
        }
        
        private void OnEnable()
        {
            if (_walkTween != null && _walkTween.IsActive())
            {
                _walkTween.Play();
            }
        }
    }
}
