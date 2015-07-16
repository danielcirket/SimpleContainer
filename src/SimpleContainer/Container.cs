﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleContainer
{
    public class Container : IContainer
    {
        private readonly ConcurrentDictionary<Type, ContainerType> _serviceTypeLookup = new ConcurrentDictionary<Type, ContainerType>();
        private readonly ConcurrentDictionary<Type, object> _serviceInstanceLookup = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, Action<object>> _serviceTypeCallbackLookup = new ConcurrentDictionary<Type, Action<object>>();

        #region IContainer Implementation

        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            _serviceTypeLookup[typeof(TService)] = new ContainerType { Type = typeof(TImplementation), IsSingleton = false };
        }
        public void Register<TService>(Type implementation, bool singleton = false)
        {
            if (implementation == null)
                return;

            _serviceTypeLookup[typeof(TService)] = new ContainerType { Type = implementation, IsSingleton = singleton };
        }
        public void Register<TService>(Type implementation, Action<TService> callback, bool singleton = false)
		{
            if (implementation == null)
                return;

            _serviceTypeLookup[typeof(TService)] = new ContainerType { Type = implementation, IsSingleton = singleton };

            if (callback != null)
                _serviceTypeCallbackLookup[typeof(TService)] = (x) => callback((TService)x);
        }
        public void Register(Type service, Type implementation, bool singleton = false)
        {
            if (service == null || implementation == null)
                return;

            if (!service.IsAssignableFrom(implementation))
                throw new ArgumentException(string.Format("Service could not be registered. {0} does not implement {1}.", implementation.Name, service.Name));

            _serviceTypeLookup[service] = new ContainerType { Type = implementation, IsSingleton = singleton };
        }
        public void Register<TService>(TService instance)
        {
            if (instance == null)
                return;

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

            // If the type isn't registered, register the type to itself.
            if (!_serviceTypeLookup.TryGetValue(type, out containerType))
            {
                Register(type, type);
                containerType = new ContainerType { Type = type, IsSingleton = false };
            }

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
                // Return null rather than throw an exception for resolve failures.
                // This null will happen when there are 0 constructors for the supplied type.
                return null;
            }            
        }

        public bool IsRegistered<TService>()
        {
            if (_serviceTypeLookup.ContainsKey(typeof(TService)) || _serviceInstanceLookup.ContainsKey(typeof(TService)))
                return true;

            return false;
        }

        #endregion

        #region IServiceProvider Implementation

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        #endregion
    }
}
