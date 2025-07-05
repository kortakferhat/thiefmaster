using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerClicker.Infrastructure
{
    public interface IService { }

    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();
        
        public static void Register<T>(T service) where T : IService
        {
            var type = typeof(T);
            _services[type] = service;
        }
        
        public static T Get<T>() where T : IService
        {
            var type = typeof(T);
            if (!_services.ContainsKey(type))
            {
                Debug.LogError($"Service of type {type.Name} is not registered.");
                return default;
            }
            
            return (T)_services[type];
        }
        
        public static void Clear()
        {
            _services.Clear();
        }
    }
} 