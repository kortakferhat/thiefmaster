using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _eventListeners = new();

        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);

            if (!_eventListeners.ContainsKey(type))
            {
                _eventListeners[type] = new List<Delegate>();
            }

            _eventListeners[type].Add(listener);
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);

            if (_eventListeners.TryGetValue(type, out var listeners))
            {
                listeners.Remove(listener);
                if (listeners.Count == 0)
                {
                    _eventListeners.Remove(type);
                }
            }
        }

        public static void Publish<T>(T publishedEvent)
        {
            var type = typeof(T);

            if (_eventListeners.TryGetValue(type, out var listeners))
            {
                var listenersCopy = new List<Delegate>(listeners);
                foreach (var listener in listenersCopy)
                {
                    ((Action<T>)listener)?.Invoke(publishedEvent);
                }
            }
        }

        public static void ClearAll()
        {
            _eventListeners.Clear();
        }
    }
}