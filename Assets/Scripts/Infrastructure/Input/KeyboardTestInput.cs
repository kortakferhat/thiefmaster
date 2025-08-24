using UnityEngine;
using UnityEngine.InputSystem;
using Infrastructure.Managers;
using Infrastructure.Managers.LevelManager;

namespace Infrastructure.Input
{
    /// <summary>
    /// MonoBehaviour component for listening to keyboard input during testing
    /// </summary>
    public class KeyboardTestInput : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private ILevelManager _levelManager;
        private PlayerInput _playerInput;
        private InputAction _restartAction;
        
        private void Awake()
        {
            // Create input action for restart
            _restartAction = new InputAction("RestartLevel", InputActionType.Button, "<Keyboard>/r");
            _restartAction.performed += OnRestartPerformed;
        }
        
        private void Start()
        {
            // Get the level manager from service locator
            _levelManager = ServiceLocator.Get<ILevelManager>();
            
            if (_levelManager == null)
            {
                Debug.LogWarning("[KeyboardTestInput] LevelManager not found in ServiceLocator");
            }
            else if (showDebugLogs)
            {
                Debug.Log("[KeyboardTestInput] Successfully connected to LevelManager");
            }
            
            // Enable the input action
            _restartAction.Enable();
        }
        
        private void OnDestroy()
        {
            // Clean up input action
            if (_restartAction != null)
            {
                _restartAction.performed -= OnRestartPerformed;
                _restartAction.Disable();
                _restartAction.Dispose();
            }
        }
        
        private void OnRestartPerformed(InputAction.CallbackContext context)
        {
            HandleRestartLevel();
        }
        
        private void HandleRestartLevel()
        {
            if (_levelManager == null)
            {
                Debug.LogWarning("[KeyboardTestInput] Cannot restart level - LevelManager not available");
                return;
            }
            
            // Trigger level restart
            _levelManager.RestartLevel();
        }
        
        /// <summary>
        /// Manually trigger restart (can be called from other scripts if needed)
        /// </summary>
        public void TriggerRestart()
        {
            HandleRestartLevel();
        }
    }
}
