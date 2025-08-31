using Gameplay.Events;
using UnityEngine;
using Infrastructure;
using Infrastructure.Input;

namespace Infrastructure.Events
{
    public class InputEvents
    {
        public class DoubleTapEvent : IBusEvent
        {
            public Vector2 TapPosition { get; }
            public float Timestamp { get; }

            public DoubleTapEvent(Vector2 tapPosition)
            {
                TapPosition = tapPosition;
                Timestamp = Time.time;
            }
        }
        
        public class SwipeEvent : IBusEvent
        {
            public SwipeDirection Direction { get; }
            public Vector2 Delta { get; }
            public float Timestamp { get; }

            public SwipeEvent(SwipeDirection direction, Vector2 delta)
            {
                Direction = direction;
                Delta = delta;
                Timestamp = Time.time;
            }
        }
    }
}
