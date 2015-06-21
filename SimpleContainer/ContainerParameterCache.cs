﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleContainer
{
    public class ContainerParameterCache
    {
        private ContainerParameterCache() { }

        #region Fields

        private static ConcurrentDictionary<ConstructorInfo, List<ParameterInfo>> _parameterCache = new ConcurrentDictionary<ConstructorInfo, List<ParameterInfo>>();

        #endregion

        #region Methods

        public static List<ParameterInfo> GetParameters(ConstructorInfo constructor)
        {
            List<ParameterInfo> parameterInfo;

            if (!_parameterCache.TryGetValue(constructor, out parameterInfo))
            {
                // Not in cache, discover and add to cache.
                parameterInfo = _parameterCache[constructor] = DiscoverParameters(constructor);
            }

            return parameterInfo;
        }
        private static List<ParameterInfo> DiscoverParameters(ConstructorInfo constructor)
        {
            return constructor.GetParameters().ToList();
        }

        #endregion
    }
}