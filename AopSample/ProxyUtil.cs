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
                var typeBuilder = _moduleBuilder.DefineType(proxyTypeName, TypeAttributes.Public, typeof(object), new[] { interfaceType });
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                var methods = interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var method in methods)
                {
                    var methodBuilder = typeBuilder.DefineMethod(method.Name
                        , MethodAttributes.Public | MethodAttributes.Virtual,
                        method.CallingConvention,
                        method.ReturnType,
                        method.GetParameters()
                            .Select(p => p.ParameterType)
                            .ToArray()
                        );
                    var ilGenerator = methodBuilder.GetILGenerator();
                    ilGenerator.EmitWriteLine($"method [{method.Name}] is invoking...");
                    if (method.ReturnType != typeof(void))
                    {
                    }
                    ilGenerator.Emit(OpCodes.Ret);
                    typeBuilder.DefineMethodOverride(methodBuilder, method);
                }

                return typeBuilder.CreateType();
            });
            return type;
        }
    }
}
