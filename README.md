# Simple Container


Usage
-----

Instantiate the Container:

```c#
var container = new Container();
```

Register a class/service:

```c#
container.Register<TService>(ImplementationInstance);
container.Register<TService, TImplementation>();
container.Register<TService>(ImplementationType);
container.Register(ServiceType, ImplementationType);
```

Register a class/service where a single instance is used when resolved:

    container.Register<TService>(ImplementationType, true);
    container.Register(ServiceType, ImplementationType, true);
    
A callback `Action<T>` can also be passed through to be used with the resolved object if required:

    container.Register<TService>(ImplementationType, callback);
   
And can also still be used as a single instance once resolved

    container.Register<TService>(ImplementationType, callback, true);


**Note:** If an instance is passed through when registered it will be treated as a single instance and the same object used each time its resolved.

Resolving an item:

    container.Resolve<T>();
    container.Resolve(ServiceType);

# **TODOs**


 - Add interface method overloads to allow callback and/or singleton parameter to be passed through in the`Register<TService, TImplementation>()`
