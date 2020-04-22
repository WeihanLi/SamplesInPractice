using System;
using System.Reflection;

namespace AopSample
{
    public class MethodInvocationContext
    {
        public MethodInfo ProxyMethod { get; }

        public MethodInfo MethodBase { get; }

        public object ProxyTarget { get; }

        public object Target { get; }

        public object[] Parameters { get; }

        public object ReturnValue { get; set; }

        public MethodInvocationContext(MethodInfo method, MethodInfo methodBase, object proxyTarget, object target, object[] parameters)
        {
            ProxyMethod = method;
            MethodBase = methodBase;
            ProxyTarget = proxyTarget;
            Target = target;
            Parameters = parameters;
        }
    }

    public static class MethodInvocationContextExtensions
    {
        public static void Invoke(this MethodInvocationContext context)
        {
            Console.WriteLine($"real method[{context.ProxyMethod.Name}] invoking...");
            var returnValue = context.MethodBase?.Invoke(context.Target, context.Parameters);
            if (null != returnValue && context.ProxyMethod.ReturnType != typeof(void))
            {
                context.ReturnValue = returnValue;
            }
        }
    }
}
