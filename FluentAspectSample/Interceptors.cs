using System;
using System.Threading.Tasks;
using WeihanLi.Common.Aspect;

namespace FluentAspectSample
{
    internal class LogInterceptor : IInterceptor
    {
        public async Task Invoke(IInvocation invocation, Func<Task> next)
        {
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} begin");
            await next();
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} end");
        }
    }

    internal class Log1Interceptor : IInterceptor
    {
        public async Task Invoke(IInvocation invocation, Func<Task> next)
        {
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} begin");
            await next();
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} end");
        }
    }

    internal class Log2Interceptor : IInterceptor
    {
        public async Task Invoke(IInvocation invocation, Func<Task> next)
        {
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} begin");
            await next();
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} end");
        }
    }

    internal class Log3Interceptor : IInterceptor
    {
        public async Task Invoke(IInvocation invocation, Func<Task> next)
        {
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} begin");
            await next();
            Console.WriteLine($"invoke {invocation.ProxyMethod} in {GetType().Name} end");
        }
    }
}
