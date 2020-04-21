using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WeihanLi.Extensions;

namespace AopSample
{
    internal static class ProxyUtil
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
                var typeBuilder = _moduleBuilder.DefineType(proxyTypeName, TypeAttributes.Public, typeof(object), new[] { interfaceType });
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                var methods = interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var method in methods)
                {
                    var methodParameterTypes = method.GetParameters()
                        .Select(p => p.ParameterType)
                        .ToArray();
                    var methodBuilder = typeBuilder.DefineMethod(method.Name
                        , MethodAttributes.Public | MethodAttributes.Virtual,
                        method.CallingConvention,
                        method.ReturnType,
                        methodParameterTypes
                        );
                    foreach (var customAttribute in method.CustomAttributes)
                    {
                        methodBuilder.SetCustomAttribute(DefineCustomAttribute(customAttribute));
                    }
                    typeBuilder.DefineMethodOverride(methodBuilder, method);

                    var il = methodBuilder.GetILGenerator();

                    var localReturnValue = il.DeclareReturnValue(method);
                    var localCurrentMethod = il.DeclareLocal(typeof(MethodInfo));
                    var localMethodBase = il.DeclareLocal(typeof(MethodInfo));
                    var localParameters = il.DeclareLocal(typeof(object[]));

                    // var currentMethod = MethodBase.GetCurrentMethod();
                    il.Call(typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod)));
                    il.EmitCastToType(typeof(MethodBase), typeof(MethodInfo));
                    il.Emit(OpCodes.Stloc, localCurrentMethod);

                    il.Emit(OpCodes.Ldloc, localCurrentMethod);
                    il.Call(typeof(Extensions).GetMethod(nameof(Extensions.GetBaseMethod), BindingFlags.Static | BindingFlags.Public));
                    il.Emit(OpCodes.Stloc, localMethodBase);

                    // var parameters = new[] {a, b, c};
                    il.Emit(OpCodes.Ldc_I4, methodParameterTypes.Length);
                    il.Emit(OpCodes.Newarr, typeof(object));
                    if (methodParameterTypes.Length > 0)
                    {
                        for (var i = 0; i < methodParameterTypes.Length; i++)
                        {
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldc_I4, i);
                            il.Emit(OpCodes.Ldarg, i + 1);
                            if (methodParameterTypes[i].IsValueType)
                            {
                                il.Emit(OpCodes.Box, methodParameterTypes[i].UnderlyingSystemType);
                            }

                            il.Emit(OpCodes.Stelem_Ref);
                        }
                    }
                    il.Emit(OpCodes.Stloc, localParameters);

                    // var aspectInvocation = new AspectInvocation(method, this, parameters);
                    var localAspectInvocation = il.DeclareLocal(typeof(MethodInvocationContext));
                    il.Emit(OpCodes.Ldloc, localCurrentMethod);
                    il.Emit(OpCodes.Ldloc, localMethodBase);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldloc, localParameters);

                    il.New(typeof(MethodInvocationContext).GetConstructors()[0]);
                    il.Emit(OpCodes.Stloc, localAspectInvocation);

                    // AspectDelegate.InvokeAspectDelegate(invocation);
                    il.Emit(OpCodes.Ldloc, localAspectInvocation);
                    var invokeAspectDelegateMethod =
                        typeof(AspectDelegate).GetMethod(nameof(AspectDelegate.InvokeAspectDelegate));

                    il.Call(invokeAspectDelegateMethod);
                    il.Emit(OpCodes.Nop);

                    if (method.ReturnType != typeof(void))
                    {
                        il.EmitCall(OpCodes.Call,
                            typeof(CoreExtension).GetMethod(nameof(CoreExtension.GetDefaultValue)),
                            new Type[] { method.ReturnType });
                        il.Emit(OpCodes.Stloc, localReturnValue);
                        il.Emit(OpCodes.Ldloc, localReturnValue);
                    }

                    il.Emit(OpCodes.Ret);
                }

                return typeBuilder.CreateType();
            });
            return type;
        }

        private static CustomAttributeBuilder DefineCustomAttribute(CustomAttributeData customAttributeData)
        {
            if (customAttributeData.NamedArguments != null)
            {
                var attributeTypeInfo = customAttributeData.AttributeType.GetTypeInfo();
                var constructor = customAttributeData.Constructor;
                //var constructorArgs = customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray();
                var constructorArgs = customAttributeData.ConstructorArguments
                    .Select(ReadAttributeValue)
                    .ToArray();
                var namedProperties = customAttributeData.NamedArguments
                        .Where(n => !n.IsField)
                        .Select(n => attributeTypeInfo.GetProperty(n.MemberName))
                        .ToArray();
                var propertyValues = customAttributeData.NamedArguments
                         .Where(n => !n.IsField)
                         .Select(n => ReadAttributeValue(n.TypedValue))
                         .ToArray();
                var namedFields = customAttributeData.NamedArguments.Where(n => n.IsField)
                         .Select(n => attributeTypeInfo.GetField(n.MemberName))
                         .ToArray();
                var fieldValues = customAttributeData.NamedArguments.Where(n => n.IsField)
                         .Select(n => ReadAttributeValue(n.TypedValue))
                         .ToArray();
                return new CustomAttributeBuilder(customAttributeData.Constructor, constructorArgs
                   , namedProperties
                   , propertyValues, namedFields, fieldValues);
            }

            return new CustomAttributeBuilder(customAttributeData.Constructor,
                customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray());
        }

        private static object ReadAttributeValue(CustomAttributeTypedArgument argument)
        {
            var value = argument.Value;
            if (argument.ArgumentType.GetTypeInfo().IsArray == false)
            {
                return value;
            }
            //special case for handling arrays in attributes
            //the actual type of "value" is ReadOnlyCollection<CustomAttributeTypedArgument>.
            var arguments = ((IEnumerable<CustomAttributeTypedArgument>)value)
                .Select(m => m.Value)
                .ToArray();
            return arguments;
        }
    }
}
