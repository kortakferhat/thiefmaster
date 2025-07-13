using UnityEngine;
using Gameplay.Configs;
using TowerClicker.Infrastructure;

namespace Gameplay.Character
{
    public class CharacterController : BaseEntity
    {
        private static readonly int Blend = Animator.StringToHash("Blend");
        public bool IsMoving => isMoving;
        public Vector3 CurrentPosition => characterTransform.position;
        
        [Header("Configuration")]
        [SerializeField] private CharacterConfigSO characterConfig;
        
        [Header("Components")]
        [SerializeField] private Transform characterTransform;
        [SerializeField] private Animator animator;
        
        private Vector3 moveDirection = Vector3.zero;
        private bool isMoving = false;
        
        public override void Initialize()
        {
            base.Initialize();
        }
        
        protected override void OnEntityUpdate()
        {
            HandleKeyboardInput();
            UpdateMovement();
            UpdateAnimator();
        }
        
        private void HandleKeyboardInput()
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                vertical += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                vertical -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal += 1f;
            
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
            isMoving = moveDirection.magnitude > 0.1f;
        }
        
        private void UpdateMovement()
        {
            if (!isMoving) return;
            
            Vector3 movement = moveDirection * characterConfig.MoveSpeed * Time.deltaTime;
            characterTransform.position += movement;
            
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                characterTransform.rotation = Quaternion.Slerp(
                    characterTransform.rotation, 
                    targetRotation, 
                    characterConfig.RotationSpeed * Time.deltaTime
                );
            }
        }
        
        private void UpdateAnimator()
        {
            if (animator != null)
            {
                var currentBlendValue = animator.GetFloat(Blend);
                var target = isMoving ? 1f : 0f;
                animator.SetFloat(Blend, Mathf.Lerp(currentBlendValue, target, Time.deltaTime * 10f));
            }
        }
        
        public void SetCharacterConfig(CharacterConfigSO config)
        {
            characterConfig = config;
        }
        
        public void SetMoveDirection(Vector3 direction)
        {
            moveDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            isMoving = moveDirection.magnitude > 0.1f;
        }
        
        public void StopMovement()
        {
            moveDirection = Vector3.zero;
            isMoving = false;
        }
    }
}
