using System;

namespace AopSample
{
    public interface IProxyGenerator
    {
        object CreateInterfaceProxy(Type serviceType);

        object CreateInterfaceProxy(Type serviceType, object implementationInstance);

        object CreateClassProxy(Type serviceType, Type implementationType, object[] args);
    }

    public class ProxyGenerator : IProxyGenerator
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        public static IProxyGenerator Instance => _generator;

        public object CreateInterfaceProxy(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object CreateInterfaceProxy(Type serviceType, object implementationInstance)
        {
            throw new NotImplementedException();
        }

        public object CreateClassProxy(Type serviceType, Type implementationType, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
