using System;
using System.Collections.Generic;

namespace Utility
{
    public class ServiceLocator
    {
        private static readonly Lazy<ServiceLocator> _instance = new Lazy<ServiceLocator>(() => new ServiceLocator());
        public static ServiceLocator Instance => _instance.Value;

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out object obj))
            {
                service = obj as T;
                return true;
            }

            service = null;
            Console.WriteLine($"ServiceLocator.Get: Service of type {type.FullName} not registered");
            return false;
        }

        public ServiceLocator Register<T>(T service)
        {
            Type type = typeof(T);

            if (!services.TryAdd(type, service))
            {
                Console.WriteLine($"ServiceLocator.Register: Service of type {type.FullName} already registered");
            }

            return this;
        }

        public ServiceLocator Register(Type type, object service)
        {
            if (!type.IsInstanceOfType(service))
            {
                throw new ArgumentException("Type of service does not match type of service interface",
                    nameof(service));
            }

            if (!services.TryAdd(type, service))
            {
                Console.WriteLine($"ServiceLocator.Register: Service of type {type.FullName} already registered");
            }

            return this;
        }

        public void Unregister<T>()
        {
            Type type = typeof(T);
            if (!services.Remove(type))
            {
                Console.WriteLine($"ServiceLocator.Unregister: No service of type {type.FullName} was registered");
            }
        }
    }
}