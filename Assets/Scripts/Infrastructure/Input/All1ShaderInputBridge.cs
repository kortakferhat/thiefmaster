using UnityEngine;
using UnityEngine.InputSystem;
using AllIn1SpriteShader;

namespace Infrastructure.Input
{
    /// <summary>
    /// Input bridge for All1ShaderDemoController to work with the new Input System
    /// This script connects the main project's Input System to the plugin's demo controller
    /// </summary>
    public class All1ShaderInputBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private All1ShaderDemoController demoController;
        
        [Header("Input Settings")]
        [SerializeField] private bool enableInput = true;
        [SerializeField] private float inputCooldown = 0.1f;
        
        private InputSystem_Actions inputActions;
        private float lastInputTime;
        
        private void Awake()
        {
            // Auto-find the demo controller if not assigned
            if (demoController == null)
            {
                demoController = FindObjectOfType<All1ShaderDemoController>();
            }
            
            if (demoController == null)
            {
                Debug.LogWarning("[All1ShaderInputBridge] No All1ShaderDemoController found! Input will be disabled.");
                enableInput = false;
                return;
            }
            
            InitializeInputSystem();
        }
        
        private void InitializeInputSystem()
        {
            inputActions = new InputSystem_Actions();
            
            // Subscribe to input events
            inputActions.Player.Move.performed += OnMovementInput;
            inputActions.Player.Previous.performed += OnPreviousInput;
            inputActions.Player.Next.performed += OnNextInput;
            
            // Enable the input actions
            inputActions.Player.Enable();
            
            Debug.Log("[All1ShaderInputBridge] Input System initialized successfully!");
        }
        
        private void OnMovementInput(InputAction.CallbackContext context)
        {
            if (!enableInput || demoController == null) return;
            if (Time.time - lastInputTime < inputCooldown) return;
            
            var input = context.ReadValue<Vector2>();
            
            // Handle horizontal movement (left/right)
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0.5f)
                {
                    demoController.OnMoveRight();
                }
                else if (input.x < -0.5f)
                {
                    demoController.OnMoveLeft();
                }
            }
            // Handle vertical movement (up/down)
            else
            {
                if (input.y > 0.5f)
                {
                    demoController.OnMoveUp();
                }
                else if (input.y < -0.5f)
                {
                    demoController.OnMoveDown();
                }
            }
            
            lastInputTime = Time.time;
        }
        
        private void OnPreviousInput(InputAction.CallbackContext context)
        {
            if (!enableInput || demoController == null) return;
            if (Time.time - lastInputTime < inputCooldown) return;
            
            demoController.OnPrevious();
            lastInputTime = Time.time;
        }
        
        private void OnNextInput(InputAction.CallbackContext context)
        {
            if (!enableInput || demoController == null) return;
            if (Time.time - lastInputTime < inputCooldown) return;
            
            demoController.OnNext();
            lastInputTime = Time.time;
        }
        
        private void OnDestroy()
        {
            // Clean up input system
            if (inputActions != null)
            {
                inputActions.Player.Move.performed -= OnMovementInput;
                inputActions.Player.Previous.performed -= OnPreviousInput;
                inputActions.Player.Next.performed -= OnNextInput;
                
                inputActions.Player.Disable();
                inputActions.Dispose();
            }
        }
        
        /// <summary>
        /// Enable or disable input handling
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            enableInput = enabled;
        }
        
        /// <summary>
        /// Set the demo controller reference
        /// </summary>
        public void SetDemoController(All1ShaderDemoController controller)
        {
            demoController = controller;
        }
    }
}
