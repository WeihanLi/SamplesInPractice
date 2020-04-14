using System;

namespace AopSample
{
    public interface IProxyGenerator
    {
        object CreateInterfaceProxy(Type interfaceType);

        object CreateInterfaceProxy(Type interfaceType, object implementationInstance);

        object CreateClassProxy(Type classType, Type implementationType, object[] args);
    }

    public static class ProxyGeneratorExtensions
    {
        public static TInterface CreateInterfaceProxy<TInterface>(this IProxyGenerator proxyGenerator) =>
            (TInterface)proxyGenerator.CreateInterfaceProxy(typeof(TInterface));
    }

    public class ProxyGenerator : IProxyGenerator
    {
        public static readonly ProxyGenerator Instance = new ProxyGenerator();

        public object CreateInterfaceProxy(Type interfaceType)
        {
            throw new NotImplementedException();
        }

        public object CreateInterfaceProxy(Type interfaceType, object implementationInstance)
        {
            throw new NotImplementedException();
        }

        public object CreateClassProxy(Type serviceType, Type implementationType, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
