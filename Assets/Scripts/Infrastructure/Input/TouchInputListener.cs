using UnityEngine;
using UnityEngine.InputSystem;
using Infrastructure;
using Infrastructure.Events;

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

    public class TouchInputListener : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float swipeResistance = 100f;
        [SerializeField] private bool enableDebugLogs = false;
        
        [Header("Double Tap Settings")]
        [SerializeField] private float doubleTapTimeWindow = 0.5f;
        [SerializeField] private float doubleTapDistanceThreshold = 50f;
        
        [Header("Input Actions")]
        [SerializeField] private InputAction position;
        [SerializeField] private InputAction press;
        
        private Vector2 initialPos;
        private Vector2 currentPos => position.ReadValue<Vector2>();
        
        // Double tap detection variables
        private float lastTapTime;
        private Vector2 lastTapPosition;
        private bool isDoubleTapDetected;
        
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
                
                // Publish swipe event via EventBus
                EventBus.Publish(new InputEvents.SwipeEvent(swipeDirection, delta));
            }
            else
            {
                // If no swipe detected, check for tap/double tap
                CheckForDoubleTap();
            }
        }
        
        private void CheckForDoubleTap()
        {
            float currentTime = Time.time;
            Vector2 currentTapPosition = currentPos;
            
            // Check if this is a potential double tap
            if (currentTime - lastTapTime <= doubleTapTimeWindow)
            {
                // Check if the tap positions are close enough
                float distance = Vector2.Distance(currentTapPosition, lastTapPosition);
                if (distance <= doubleTapDistanceThreshold)
                {
                    // Double tap detected!
                    if (enableDebugLogs)
                        Debug.Log($"[SwipeInputHandler] Double tap detected at position: {currentTapPosition}");
                    
                    // Publish double tap event via EventBus
                    EventBus.Publish(new InputEvents.DoubleTapEvent(currentTapPosition));
                    isDoubleTapDetected = true;
                    
                    // Reset to prevent multiple double tap events
                    lastTapTime = 0f;
                    return;
                }
            }
            
            // Update last tap info for next potential double tap
            lastTapTime = currentTime;
            lastTapPosition = currentTapPosition;
            isDoubleTapDetected = false;
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
        
        // Public methods to configure double tap settings
        public void SetDoubleTapTimeWindow(float timeWindow)
        {
            doubleTapTimeWindow = Mathf.Max(0.1f, timeWindow);
        }
        
        public void SetDoubleTapDistanceThreshold(float distance)
        {
            doubleTapDistanceThreshold = Mathf.Max(10f, distance);
        }
    }
}
