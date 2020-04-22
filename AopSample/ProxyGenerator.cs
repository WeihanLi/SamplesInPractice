using System;
using System.Linq;
using System.Reflection;
using WeihanLi.Extensions;

namespace AopSample
{
    public static class Extensions
    {
        public static TInterface CreateInterfaceProxy<TInterface>(this ProxyGenerator proxyGenerator) =>
            (TInterface)proxyGenerator.CreateInterfaceProxy(typeof(TInterface));

        public static TInterface CreateInterfaceProxy<TInterface, TImplement>(this ProxyGenerator proxyGenerator) where TImplement : TInterface =>
            (TInterface)proxyGenerator.CreateInterfaceProxy(typeof(TInterface), typeof(TImplement));

        public static TClass CreateClassProxy<TClass>(this ProxyGenerator proxyGenerator) where TClass : class =>
            (TClass)proxyGenerator.CreateClassProxy(typeof(TClass));

        public static TClass CreateClassProxy<TClass, TImplement>(this ProxyGenerator proxyGenerator) where TImplement : TClass =>
            (TClass)proxyGenerator.CreateClassProxy(typeof(TClass), typeof(TImplement));

        public static MethodBase GetBaseMethod(this MethodBase currentMethod)
        {
            if (null == currentMethod?.DeclaringType)
                return null;

            var parameters = currentMethod.GetParameters().Select(o => o.ParameterType).ToArray();

            var baseTypeMethod = currentMethod.DeclaringType.BaseType?
                .GetMethod(
                    currentMethod.Name,
                    parameters
                );
            if (null != baseTypeMethod)
                return baseTypeMethod;

            foreach (var interfaceType in currentMethod.DeclaringType.BaseType.GetImplementedInterfaces())
            {
                baseTypeMethod = interfaceType.GetMethod(currentMethod.Name, parameters);
                if (null != baseTypeMethod)
                {
                    return baseTypeMethod;
                }
            }

            return null;
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
            var type = ProxyUtil.CreateClassProxy(classType, interfaceTypes);
            return Activator.CreateInstance(type);
        }

        public object CreateClassProxy(Type classType, Type implementationType, params Type[] interfaceTypes)
        {
            var type = ProxyUtil.CreateClassProxy(classType, interfaceTypes);
            return Activator.CreateInstance(type);
        }
    }
}
