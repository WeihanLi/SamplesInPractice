using System;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using WeihanLi.Common;

namespace ConsoleSample.Interceptors
{
    internal class CustomInterceptorAttribute : AbstractInterceptorAttribute
    {
        private static int _counter;
        private readonly ILogger _logger = DependencyResolver.Current.GetService<ILogger>();


        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            if (_counter > 0)
            {
                _logger.Info($"service {context.Implementation.GetType().FullName}.{context.ImplementationMethod.Name} call count greater than 1");
                return;
            }

            try
            {
                _logger.Info($"service {context.Implementation.GetType().FullName}.{context.ImplementationMethod.Name} call start");
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.Info($"Service threw an exception!Exception message:{ex.Message}");
                throw;
            }
            finally
            {
                _logger.Info($"service {context.Implementation.GetType().FullName}.{context.ImplementationMethod.Name} call end");

                Interlocked.Increment(ref _counter);
            }
        }
    }
}
