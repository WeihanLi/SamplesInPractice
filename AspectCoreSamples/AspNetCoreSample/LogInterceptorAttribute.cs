using AspectCore.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace AspNetCoreSample
{
    internal class LogInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            try
            {
                Console.WriteLine("service call start");
                await next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service threw an exception!Exception message:{ex.Message}");
                throw;
            }
            finally
            {
                Console.WriteLine("service call end");
            }
        }
    }
}