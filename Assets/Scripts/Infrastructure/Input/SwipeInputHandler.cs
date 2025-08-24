using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Infrastructure.Input
{
    public enum SwipeDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public class SwipeInputHandler : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float swipeResistance = 100f;
        [SerializeField] private bool enableDebugLogs = false;
        
        [Header("Events")]
        public UnityEvent<SwipeDirection> OnSwipeDetected;
        
        [Header("Input Actions")]
        [SerializeField] private InputAction position;
        [SerializeField] private InputAction press;
        
        private Vector2 initialPos;
        private Vector2 currentPos => position.ReadValue<Vector2>();
        
        private void Awake()
        {
            // Enable input actions
            position.Enable();
            press.Enable();
            
            // Subscribe to press events
            press.performed += _ => { initialPos = currentPos; };
            press.canceled += _ => DetectSwipe();
            
            if (enableDebugLogs)
                Debug.Log("[SwipeInputHandler] Initialized with swipe resistance: " + swipeResistance);
        }
        
        private void DetectSwipe()
        {
            Vector2 delta = currentPos - initialPos;
            Vector2 direction = Vector2.zero;
         
            Debug.Log($"[SwipeInputHandler] Swipe detected: DetectSwipe (Delta: {delta})");

            
            if (Mathf.Abs(delta.x) > swipeResistance)
            {
                direction.x = Mathf.Clamp(delta.x, -1, 1);
            }
            if (Mathf.Abs(delta.y) > swipeResistance)
            {
                direction.y = Mathf.Clamp(delta.y, -1, 1);
            }
            
            if (direction != Vector2.zero)
            {
                SwipeDirection swipeDirection = GetSwipeDirection(direction);
                
                if (enableDebugLogs)
                    Debug.Log($"[SwipeInputHandler] Swipe detected: {swipeDirection} (Delta: {delta})");
                
                OnSwipeDetected?.Invoke(swipeDirection);
            }
        }
        
        private SwipeDirection GetSwipeDirection(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return direction.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                return direction.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }
        
        private void OnDestroy()
        {
            if (position != null) position.Disable();
            if (press != null) press.Disable();
        }
    }
}
