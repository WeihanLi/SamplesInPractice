using System;
using System.Reflection;

namespace AopSample
{
    public class MethodInvocationContext
    {
        public MethodInfo Method { get; }

        public MethodInfo MethodBase { get; }

        public object Target { get; }

        public object[] Parameters { get; }

        public object ReturnValue { get; set; }

        public MethodInvocationContext(MethodInfo method, MethodInfo methodBase, object target, object[] parameters)
        {
            Method = method;
            MethodBase = methodBase;
            Target = target;
            Parameters = parameters;
        }
    }

    public static class MethodInvocationContextExtensions
    {
        public static object Invoke(this MethodInvocationContext context)
        {
            Console.WriteLine($"real method[{context.Method.Name}] invoking...");
            return context.MethodBase?.Invoke(context.Target, context.Parameters);
        }
    }
}
