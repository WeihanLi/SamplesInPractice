using System;
using System.Reflection;

namespace AopSample
{
    public class MethodInvocationContext
    {
        public MethodInfo MethodInfo { get; set; }

        public object Target { get; set; }

        public object[] Parameters { get; set; }

        public Type[] GenericParameters { get; set; }
    }

    public static class MethodInvocationContextExtensions
    {
        public static object Invoke(this MethodInvocationContext context) =>
            context.MethodInfo?.Invoke(context.Target, context.Parameters);
    }
}
