using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleContainer
{
    public class Container : IContainer
    {
        #region Fields

        private readonly ConcurrentDictionary<Type, ContainerType> _serviceTypeLookup = new ConcurrentDictionary<Type, ContainerType>();
        private readonly ConcurrentDictionary<Type, object> _serviceInstanceLookup = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, Action<object>> _serviceTypeCallbackLookup = new ConcurrentDictionary<Type, Action<object>>();

        #endregion

        #region Methods

        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            _serviceTypeLookup[typeof(TService)] = new ContainerType { Type = typeof(TImplementation), IsSingleton = false };
        }
        public void Register<TService>(Type implementation)
        {
            Register<TService>(implementation, false);
        }
        public void Register<TService>(Type implementation, bool singleton)
        {
            _serviceTypeLookup[typeof(TService)] = new ContainerType { Type = implementation, IsSingleton = singleton };
        }
        public void Register<TService>(Type implementation, Action<TService> callback)
        {
            Register(implementation, callback, false);
        }
        public void Register<TService>(Type implementation, Action<TService> callback, bool singleton)
        {
            _serviceTypeLookup[typeof(TService)] = new ContainerType { Type = implementation, IsSingleton = singleton };
            _serviceTypeCallbackLookup[typeof(TService)] = (x) => callback((TService)x);
        }
        public void Register(Type service, Type implementation)
        {
            Register(service, implementation, false);
        }
        public void Register(Type service, Type implementation, bool singleton)
        {
            if (service == null)
                throw new ArgumentNullException("service", string.Format("Service not registered. The type could not be resolved."));

            if (implementation == null)
                throw new ArgumentNullException("implementation", string.Format("Service not registered. The type for {0} could not be resolved.", service.Name));

            if (!service.IsAssignableFrom(implementation))
                throw new ArgumentException(string.Format("Service could not be registered. {0} does not implement {1}.", implementation.Name, service.Name));

            _serviceTypeLookup[service] = new ContainerType { Type = implementation, IsSingleton = singleton };
        }
        /// <summary>
        /// Allows you to register an instance of an object to a type.
        /// This will then be used as a singleton, and the same object passed
        /// for each call.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="instance"></param>
        public void Register<TService>(TService instance)
        {
            _serviceInstanceLookup[typeof(TService)] = instance;
        }
        public T Resolve<T>()
         {
            return (T)Resolve(typeof(T));
        }
        private object Resolve(Type type)
        {
            ContainerType containerType;
            object instance = null;

            if (!_serviceTypeLookup.TryGetValue(type, out containerType) && !_serviceInstanceLookup.TryGetValue(type, out instance))
                throw new Exception(string.Format("Service not registered. The type {0} could not be resolved.", type));

            // TODO: Should it use the instance by default? I'd assume so initially.
            // Check if the service has an instance in the list of instances, if so, return it here.
            if (_serviceInstanceLookup.TryGetValue(type, out instance))
                return instance;

            var constructor = ContainerConstructorCache.GetConstructor(containerType.Type);
            if (constructor != null)
            {
                // TODO: Deal with multiple constructors!?

                // Get constructor parameters.
                var parameters = ContainerParameterCache.GetParameters(constructor);
                var parameterObjects = new List<object>();
         
                foreach (var parameter in parameters)
                {
                    parameterObjects.Add(Resolve(parameter.ParameterType));
                }

                var obj = Activator.CreateInstance(containerType.Type, parameterObjects.ToArray());

                Action<object> callback;
                if (_serviceTypeCallbackLookup.TryGetValue(type, out callback))
                    callback(obj);

                if (containerType.IsSingleton)
                    _serviceInstanceLookup[type] = obj;

                return obj;
            }
            else
            {
                throw new Exception(string.Format("No constructors found for type: {0}.", containerType.Type));
            }            
        }

        /// <summary>
        /// TODO: Probably not required, might remove.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public bool IsRegistered<TService>()
        {
            try
            {
                var service = Resolve<TService>();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
