using System;

namespace AopSample
{
    public static class ProxyGeneratorExtensions
    {
        public static TInterface CreateInterfaceProxy<TInterface>(this ProxyGenerator proxyGenerator) =>
            (TInterface)proxyGenerator.CreateInterfaceProxy(typeof(TInterface));
    }

    public class ProxyGenerator
    {
        public static readonly ProxyGenerator Instance = new ProxyGenerator();

        public object CreateInterfaceProxy(Type interfaceType)
        {
            var type = ProxyUtil.CreateInterfaceProxy(interfaceType);
            return Activator.CreateInstance(type);
        }

        public object CreateInterfaceProxy(Type interfaceType, object implementationInstance)
        {
            throw new NotImplementedException();
        }

        public object CreateClassProxy(Type classType, params Type[] interfaceTypes)
        {
            throw new NotImplementedException();
        }

        public object CreateClassProxy(Type classType, Type implementationType, params Type[] interfaceTypes)
        {
            throw new NotImplementedException();
        }
    }
}
