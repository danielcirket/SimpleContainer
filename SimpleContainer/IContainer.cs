﻿using System;

namespace SimpleContainer
{
    public interface IContainer
    {
        void Register<TService, TImplementation>() where TImplementation : TService;
        void Register<TService>(Type implementation);
        void Register<TService>(Type implementation, bool singleton);
        void Register<TService>(Type implementation, Action<TService> callback, bool singleton);
        void Register(Type service, Type implementation);
        void Register(Type service, Type implementation, bool singleton);
        void Register<TService>(TService instance);
        T Resolve<T>();
        bool IsRegistered<T>();
    }
}