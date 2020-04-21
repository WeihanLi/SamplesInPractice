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
            var action = _aspectDelegates.GetOrAdd(context.Method.ToString(), m =>
            {
                var builder = PipelineBuilder.Create<MethodInvocationContext>(x =>
                {
                    x.ReturnValue = x.Invoke();
                });
                if (context.Method != null)
                {
                    foreach (var aspect in context.Method.GetCustomAttributes<AbstractAspect>(true))
                    {
                        builder.Use((x, next) =>
                        {
                            aspect.Invoke(x);
                            next();
                        });
                    }
                }
                return builder.Build();
            });
            action.Invoke(context);
        }
    }
}
