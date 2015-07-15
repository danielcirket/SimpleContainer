using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SimpleContainer
{
    internal static class TypeResolver
    {
        #region Fields

        private static ConcurrentDictionary<Type, Type> _typeCache = new ConcurrentDictionary<Type, Type>();

        #endregion

        #region Methods

        public static Type Resolve<T>(string className)
        {
            var type = typeof(T);
            Type implementationType = null;

            if (!_typeCache.TryGetValue(type, out implementationType))
            {
                implementationType = 
                    _typeCache[type] = Resolve(className, type);
            }

            return implementationType;
        }
        /// <summary>
        /// Passing in the serviceType parameter will ensure that the implementing type implements the service class/interface.
        /// </summary>
        /// <param name="implementingType"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static Type Resolve(string implementingType, Type serviceType = null)
        {
            // TODO: Implement some caching for this?

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var subscriberAssemblies = assemblies.Where(a => !a.FullName.StartsWith("SubscriberCRM.Client", StringComparison.OrdinalIgnoreCase));
            var types = subscriberAssemblies.SelectMany(a => a.GetTypes());

            Type type = null;

            if (serviceType != null)
            {
                type = types.FirstOrDefault(t => t.Name == implementingType && serviceType.IsAssignableFrom(t));
            }
            else
            {
                type = types.FirstOrDefault(t => t.Name == implementingType);
             }

            return type;
        }
        #endregion
    }
}

