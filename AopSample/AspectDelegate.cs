using System;
using System.Collections.Concurrent;
using System.Reflection;
using WeihanLi.Common.Helpers;

namespace AopSample
{
    public class AspectDelegate
    {
        private static readonly ConcurrentDictionary<string, Action<MethodInvocationContext>> _aspectDelegates = new ConcurrentDictionary<string, Action<MethodInvocationContext>>();

        public static void InvokeAspectDelegate(MethodInvocationContext context)
        {
            var action = _aspectDelegates.GetOrAdd($"{context.Method.DeclaringType}.{context.Method}", m =>
            {
                var builder = PipelineBuilder.Create<MethodInvocationContext>(x => x.Invoke());
                if (context.Method != null)
                {
                    foreach (var aspect in context.Method.GetCustomAttributes<AbstractAspect>(true))
                    {
                        builder.Use(aspect.Invoke);
                    }
                }
                return builder.Build();
            });
            action.Invoke(context);

            // check for return value
            if (context.Method.ReturnType != typeof(void))
            {
                if (context.ReturnValue == null && context.Method.ReturnType.IsValueType)
                {
                    context.ReturnValue = Activator.CreateInstance(context.Method.ReturnType);
                }
            }
        }
    }
}
