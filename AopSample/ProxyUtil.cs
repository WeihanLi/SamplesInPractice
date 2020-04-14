using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WeihanLi.Extensions;

namespace AopSample
{
    internal class ProxyUtil
    {
        private const string ProxyAssemblyName = "Aop.DynamicGenerated";
        private static readonly ModuleBuilder _moduleBuilder;
        private static readonly ConcurrentDictionary<string, Type> _proxyTypes = new ConcurrentDictionary<string, Type>();

        static ProxyUtil()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ProxyAssemblyName), AssemblyBuilderAccess.Run);
            _moduleBuilder = asmBuilder.DefineDynamicModule("Default");
        }

        public static Type CreateInterfaceProxy(Type interfaceType)
        {
            var proxyTypeName = $"{ProxyAssemblyName}.{interfaceType.FullName}";
            var type = _proxyTypes.GetOrAdd(proxyTypeName, name =>
            {
                //
                var typeBuilder = _moduleBuilder.DefineType(proxyTypeName, TypeAttributes.Public);
                //
                var ctorBuilder = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                var methods = interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var method in methods)
                {
                    var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public, CallingConventions.HasThis,
                        method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
                    foreach (var aspect in method.GetCustomAttributes<AbstractAspect>())
                    {
                        var ilGenerator = methodBuilder.GetILGenerator();
                        ilGenerator.EmitWriteLine("testing");
                        ilGenerator.Emit(new OpCode()
                        {
                        });

                        aspect.Invoke(new MethodInvocationContext()
                        {
                            MethodInfo = method,
                        });
                    }
                }

                return interfaceType;
            });
            return type;
        }

        public static Type CreateClassProxy(Type classType, params Type[] interfaceType)
        {
            var proxyTypeName = $"{ProxyAssemblyName}.{classType.FullName}__{(interfaceType ?? Enumerable.Empty<Type>()).OrderBy(t => t.FullName).StringJoin("__").Replace('.', '_')}";
            var type = _proxyTypes.GetOrAdd(proxyTypeName, name =>
            {
                //
                var typeBuilder = _moduleBuilder.DefineType(proxyTypeName, TypeAttributes.Public, classType);
                //
                var ctorBuilder = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                return classType;
            });
            return type;
        }
    }
}
