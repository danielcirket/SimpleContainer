#**Simple Container**

----------

Usage
-----

Register a class/service:

	Container.Register<TService>(TImplementationInstance);
    Container.Register<TService, TImplementation>();
    Container.Register<TService>(TImplementationType);
    Container.Register(TService, TImplementationType);

Register a class/service where a single instance is used when resolved:

    Container.Register<TService>(TImplementationType, true);
    Container.Register(TService, TImplementationType, true);
    
A callback (Action< T >) can also be passed through to be used with the resolved object if required:

    Container.Register<TService>(TImplementationType, callback);
   
And can also still be used as a single instance once resolved

    Container.Register<TService>(TImplementationType, callback, true);


**Note:** If an instance is passed through when registered it will be treated as a single instance and the same object used each time its resolved.

Resolving an item:

    Container.Resolve<T>();
    Container.Resolve(TServiceType);

#**TODOs**

----------

 - Add interface method overloads to allow callback and/or singleton parameter to be passed through in the`Register<TService, TImplementation>()`
