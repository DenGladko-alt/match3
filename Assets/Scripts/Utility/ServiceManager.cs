using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class ServiceManager : MonoBehaviourSingleton<ServiceManager> 
    {
        
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (var service in services)
                {
                    Debug.Log(service.Value);
                }
            }
        }

        public bool TryGet<T>(out T service) where T : class {
            Type type = typeof(T);
            if (services.TryGetValue(type, out object obj)) {
                service = obj as T;
                return true;
            }

            service = null;
            Debug.LogError($"ServiceManager.Get: Service of type {type.FullName} not registered");
            return false;
        }

        public ServiceManager Register<T>(T service) {
            Type type = typeof(T);
        
            if (!services.TryAdd(type, service)) {
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");
            }
        
            return this;
        }

        public ServiceManager Register(Type type, object service) {
            if (!type.IsInstanceOfType(service)) {
                throw new ArgumentException("Type of service does not match type of service interface", nameof(service));
            }
        
            if (!services.TryAdd(type, service)) {
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");
            }
        
            return this;
        }
        
        public void RemoveService<T>()
        {
            Type type = typeof(T);
            services.Remove(type);
        }
    }
}
