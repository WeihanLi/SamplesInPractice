using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace AopSample
{
    public class AspectDelegate
    {
        private static readonly ConcurrentDictionary<string, Action<MethodInvocationContext>> _aspectDelegates = new ConcurrentDictionary<string, Action<MethodInvocationContext>>();

        public static void InvokeAspectDelegate(MethodInvocationContext context)
        {
            var action = _aspectDelegates.GetOrAdd($"{context.ProxyMethod.DeclaringType}.{context.ProxyMethod}", m =>
            {
                var aspects = new List<AbstractAspect>(8);
                if (context.MethodBase != null)
                {
                    foreach (var aspect in context.MethodBase.GetCustomAttributes<AbstractAspect>())
                    {
                        if (!aspects.Exists(x => x.GetType() == aspect.GetType()))
                        {
                            aspects.Add(aspect);
                        }
                    }
                }

                var methodParameterTypes = context.ProxyMethod.GetParameters().Select(p => p.GetType()).ToArray();
                foreach (var implementedInterface in context.ProxyTarget.GetType().GetImplementedInterfaces())
                {
                    var method = implementedInterface.GetMethod(context.ProxyMethod.Name, methodParameterTypes);
                    if (null != method)
                    {
                        foreach (var aspect in method.GetCustomAttributes<AbstractAspect>())
                        {
                            if (!aspects.Exists(x => x.GetType() == aspect.GetType()))
                            {
                                aspects.Add(aspect);
                            }
                        }
                    }
                }

                var builder = PipelineBuilder.Create<MethodInvocationContext>(x => x.Invoke());
                foreach (var aspect in aspects)
                {
                    builder.Use(aspect.Invoke);
                }
                return builder.Build();
            });
            action.Invoke(context);

            // check for return value
            if (context.ProxyMethod.ReturnType != typeof(void))
            {
                if (context.ReturnValue == null && context.ProxyMethod.ReturnType.IsValueType)
                {
                    context.ReturnValue = Activator.CreateInstance(context.ProxyMethod.ReturnType);
                }
            }
        }
    }
}
