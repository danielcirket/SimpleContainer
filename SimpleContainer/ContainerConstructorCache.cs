using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace SimpleContainer
{
    internal class ContainerConstructorCache
    {
        private ContainerConstructorCache() { }

        #region Fields

        private static ConcurrentDictionary<Type, ConstructorInfo> _constructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();

        #endregion

        #region Methods

        public static ConstructorInfo GetConstructor(Type type)
        {
            ConstructorInfo constructor;

            if (!_constructorCache.TryGetValue(type, out constructor))
            {
                // Not in cache, discover and add to cache.
                constructor = _constructorCache[type] = DiscoverConstructor(type);
            }

            return constructor;
        }
        private static ConstructorInfo DiscoverConstructor(Type type)
        {
            BindingFlags flags = BindingFlags.Public |
                             BindingFlags.NonPublic |
                             BindingFlags.Static |
                             BindingFlags.Instance |
                             BindingFlags.DeclaredOnly;

            ConstructorInfo[] constructors = type.GetConstructors(flags);

            if (constructors.Count() > 0)
            {
                // If constructors found return first constructor
                // TODO: May want to change this to support multiple constructors?!?
                return constructors[0];
            }

            return null;
        }

        #endregion
    }
}
