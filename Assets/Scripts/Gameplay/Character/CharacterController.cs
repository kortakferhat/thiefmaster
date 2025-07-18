using Gameplay.Collectables;
using UnityEngine;
using UnityEngine.InputSystem;
using Gameplay.Configs;

namespace Gameplay.Character
{
    public class CharacterController : BaseEntity
    {
        private static readonly int Blend = Animator.StringToHash("Blend");
        public bool IsMoving => _isMoving;
        public Vector3 CurrentPosition => characterTransform.position;
        
        [Header("Configuration")]
        [SerializeField] private CharacterConfigSO characterConfig;
        
        [Header("Components")]
        [SerializeField] private Transform characterTransform;
        [SerializeField] private Animator animator;
        
        private Rigidbody _rigidbody;
        private InputSystem_Actions _playerInputActions;
        private Vector3 _moveDirection = Vector3.zero;
        private Vector2 _inputVector = Vector2.zero;
        private bool _isMoving = false;
        
        public override void Initialize()
        {
            base.Initialize();
            _rigidbody = GetComponent<Rigidbody>();
            
            // Initialize Input System
            _playerInputActions = new InputSystem_Actions();
            _playerInputActions.Player.Move.performed += OnMovementPerformed;
            _playerInputActions.Player.Move.canceled += OnMovementCanceled;
            _playerInputActions.Player.Enable();
        }
        
        private void OnDestroy()
        {
            if (_playerInputActions != null)
            {
                _playerInputActions.Player.Move.performed -= OnMovementPerformed;
                _playerInputActions.Player.Move.canceled -= OnMovementCanceled;
                _playerInputActions.Player.Disable();
                _playerInputActions.Dispose();
            }
        }
        
        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            _inputVector = context.ReadValue<Vector2>();
            HandleMovementInput();
        }
        
        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            _inputVector = Vector2.zero;
            HandleMovementInput();
        }
        
        private void HandleMovementInput()
        {
            _moveDirection = new Vector3(_inputVector.x, 0f, _inputVector.y).normalized;
            _isMoving = _moveDirection.magnitude > 0.1f;
        }
        
        protected override void OnEntityUpdate()
        {
            UpdateAnimator();
        }

        protected override void OnEntityFixedUpdate()
        {
            base.OnEntityFixedUpdate();

            if (!_isMoving)
            {
                _rigidbody.linearVelocity = Vector3.zero; 
                return;
            }

            var movement = _moveDirection * (characterConfig.MoveSpeed * 10 * Time.fixedDeltaTime);
            _rigidbody.linearVelocity = movement;

            if (_moveDirection != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(_moveDirection);
                _rigidbody.rotation = Quaternion.Slerp(
                    _rigidbody.rotation,
                    targetRotation,
                    characterConfig.RotationSpeed * Time.fixedDeltaTime
                );
            }
            
            _rigidbody.angularVelocity = Vector3.zero;
        }
        
        
        private void UpdateAnimator()
        {
            if (animator != null)
            {
                var currentBlendValue = animator.GetFloat(Blend);
                var target = _isMoving ? 1f : 0f;
                animator.SetFloat(Blend, Mathf.Lerp(currentBlendValue, target, Time.deltaTime * 10f));
            }
        }

        protected override void OnEntityTriggerEnter(Collider other)
        {
            base.OnEntityTriggerEnter(other);

            if (other.tag.Equals("Item"))
            {
                var collectable = other.GetComponent<IItem>();
                collectable.Collect();
            }
        }
    }
}
