using System;
using System.Linq;
using System.Reflection;

namespace AopSample
{
    public static class Extensions
    {
        public static TInterface CreateInterfaceProxy<TInterface>(this ProxyGenerator proxyGenerator) =>
            (TInterface)proxyGenerator.CreateInterfaceProxy(typeof(TInterface));

        public static TInterface CreateInterfaceProxy<TInterface, TImplement>(this ProxyGenerator proxyGenerator) where TImplement : TInterface =>
            (TInterface)proxyGenerator.CreateInterfaceProxy(typeof(TInterface), typeof(TImplement));

        public static MethodBase GetBaseMethod(this MethodBase currentMethod)
        {
            return currentMethod?.DeclaringType?.BaseType?
                .GetMethod(
                    currentMethod.Name,
                    currentMethod.GetParameters().Select(o => o.ParameterType).ToArray()
                );
        }
    }

    public class ProxyGenerator
    {
        public static readonly ProxyGenerator Instance = new ProxyGenerator();

        public object CreateInterfaceProxy(Type interfaceType)
        {
            var type = ProxyUtil.CreateInterfaceProxy(interfaceType);
            return Activator.CreateInstance(type);
        }

        public object CreateInterfaceProxy(Type interfaceType, Type implementationType)
        {
            var type = ProxyUtil.CreateInterfaceProxy(interfaceType, implementationType);
            return Activator.CreateInstance(type);
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
