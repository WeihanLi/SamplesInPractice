using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AopSample
{
    internal static class ILGeneratorExtensions
    {
        public static void EmitCastToType(this ILGenerator ilGenerator, Type typeFrom, Type typeTo)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (!typeFrom.IsValueType && typeTo.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Unbox_Any, typeTo);
            }
            else if (typeFrom.IsValueType && !typeTo.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, typeFrom);
                if (typeTo != typeof(object))
                {
                    ilGenerator.Emit(OpCodes.Castclass, typeTo);
                }
            }
            else if (!typeFrom.IsValueType && !typeTo.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Castclass, typeTo);
            }
            else
            {
                throw new InvalidCastException($"Caanot cast {typeFrom} to {typeTo}.");
            }
        }

        public static LocalBuilder DeclareReturnValue(this ILGenerator il, MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
            {
                return il.DeclareLocal(method.ReturnType);
            }

            return null;
        }

        public static void Return(this ILGenerator il, MethodInfo method, LocalBuilder local)
        {
            if (method.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, local);
            }

            il.Emit(OpCodes.Ret);
        }

        public static void LoadMethodParameters(this ILGenerator il, MethodInfo method)
        {
            il.Emit(OpCodes.Ldarg_0);
            for (var i = 1; i <= method.GetParameters().Length; i++)
            {
                il.Emit(OpCodes.Ldarg, i);
            }
        }

        public static void Call(this ILGenerator il, MethodInfo method)
        {
            il.Emit(OpCodes.Call, method);
        }

        public static void Call(this ILGenerator il, ConstructorInfo constructor)
        {
            il.Emit(OpCodes.Call, constructor);
        }

        public static void CallVirt(this ILGenerator il, MethodInfo method)
        {
            il.Emit(OpCodes.Callvirt, method);
        }

        public static void New(this ILGenerator il, ConstructorInfo constructor)
        {
            il.Emit(OpCodes.Newobj, constructor);
        }
    }
}
