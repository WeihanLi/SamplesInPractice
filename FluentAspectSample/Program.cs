using System;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Aspect;

namespace FluentAspectSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddFluentAspects(options =>
            {
                options.InterceptAll()
                    .With<LogInterceptor>()
                    ;
                options.NoInterceptType<Svc3>();
                options.NoInterceptMethod<Svc4>(s => s.Invoke3());
            });
            services.AddTransientProxy<Svc4>();
            var serviceProvider = services.BuildServiceProvider();
            var proxyFactory = serviceProvider.GetRequiredService<IProxyFactory>();

            var svc1 = proxyFactory.CreateProxy<ISvc1>();
            svc1.Invoke();
            Console.WriteLine();

            var svc2 = proxyFactory.CreateProxy<ISvc2, Svc2>();
            svc2.Invoke();
            Console.WriteLine();

            var svc3 = proxyFactory.CreateProxy<Svc3>();
            svc3.Invoke();
            Console.WriteLine();

            var svc4 = proxyFactory.CreateProxyWithTarget<ISvc2, Svc2>(new Svc2());
            svc4.Invoke();
            Console.WriteLine();

            // 直接从注册的服务中获取
            var svc5 = serviceProvider.GetRequiredService<Svc4>();
            svc5.Invoke();
            Console.WriteLine();
            svc5.Invoke2();
            Console.WriteLine();
            svc5.Invoke3();
            Console.WriteLine();

            Console.WriteLine("finished");
            Console.ReadLine();
        }
    }
}
