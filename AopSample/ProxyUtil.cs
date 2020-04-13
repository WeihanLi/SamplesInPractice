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
        private readonly ModuleBuilder _moduleBuilder;
        private readonly ConcurrentDictionary<string, Type> _proxyTypes = new ConcurrentDictionary<string, Type>();

        public ProxyUtil()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ProxyAssemblyName), AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = asmBuilder.DefineDynamicModule("Default");
        }

        public Type CreateClassProxy(Type classType, params Type[] interfaceType)
        {
            var proxyTypeName = $"{ProxyAssemblyName}.{classType.FullName}.{(interfaceType ?? Enumerable.Empty<Type>()).OrderBy(t => t.FullName).StringJoin("_").Replace('.', '_')}";
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
