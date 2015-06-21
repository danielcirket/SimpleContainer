using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleContainer
{
    public class Container
    {
        #region Fields

        private readonly ConcurrentDictionary<Type, Type> _serviceTypeLookup = new ConcurrentDictionary<Type, Type>();
        private readonly ConcurrentDictionary<Type, object> _serviceInstanceLookup = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, Action<object>> _serviceTypeCallbackLookup = new ConcurrentDictionary<Type, Action<object>>();

        #endregion

        #region Methods

        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            _serviceTypeLookup[typeof(TService)] = typeof(TImplementation);
        }
        public void Register<TService>(Type implementation)
        {
            _serviceTypeLookup[typeof(TService)] = implementation;
        }
        public void Register<TService>(Type implementation, Action<TService> callback)
        {
            _serviceTypeLookup[typeof(TService)] = implementation;
            _serviceTypeCallbackLookup[typeof(TService)] = x => callback((TService)x);
        }
        public void Register(Type service, Type implementation)
        {
            if (service == null)
                throw new ArgumentNullException("service", string.Format("Service not registered. The type could not be resolved."));

            if (implementation == null)
                throw new ArgumentNullException("implementation", string.Format("Service not registered. The type for {0} could not be resolved.", service.Name));

            _serviceTypeLookup[service] = implementation;
        }
        /// <summary>
        /// Allows you to register an instance of an object to a type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="instance"></param>
        public void Register<TService>(TService instance)
        {
            if (instance == null)
                throw new ArgumentNullException(string.Format("Instance could not be registered. The instance for type {0} could not be resolved.", typeof(TService).Name));

            _serviceInstanceLookup[typeof(TService)] = instance;
        }
        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }
        private object Resolve(Type type)
        {
            Type implementationType = null;
            object instance;

            if (!_serviceTypeLookup.TryGetValue(type, out implementationType) && !_serviceInstanceLookup.TryGetValue(type, out instance))
                throw new Exception(string.Format("Service not registered. The type {0} could not be resolved.", type));

            // TODO: Should it use the instance by default? I'd assume so initially.
            // Check if the service has an instance in the list of instances, if so, return it here.
            if (_serviceInstanceLookup.TryGetValue(type, out instance))
            {
                return instance;
            }

            var constructor = ContainerConstructorCache.GetConstructor(implementationType);
            if (constructor != null)
            {
                // TODO: Deal with multiple constructors.

                // Get constructor parameters.
                var parameters = ContainerParameterCache.GetParameters(constructor);
                var parameterObjects = new List<object>();

                foreach (var parameter in parameters)
                {
                    parameterObjects.Add(Resolve(parameter.ParameterType));
                }

                var obj = Activator.CreateInstance(implementationType, parameterObjects.ToArray());

                Action<object> callback;
                if (_serviceTypeCallbackLookup.TryGetValue(type, out callback))
                {
                    callback(obj);
                }

                return obj;
            }
            else
            {
                throw new Exception(string.Format("No constructors found for type: {0}.", implementationType));
            }
        }

        #endregion
    }
}
