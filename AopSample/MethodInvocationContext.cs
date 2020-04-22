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
        public static void Invoke(this MethodInvocationContext context)
        {
            Console.WriteLine($"real method[{context.Method.Name}] invoking...");
            var returnValue = context.MethodBase?.Invoke(context.Target, context.Parameters);
            if (null != returnValue && context.Method.ReturnType != typeof(void))
            {
                context.ReturnValue = returnValue;
            }
        }
    }
}
