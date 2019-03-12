using System;
using AspectCore.Configuration;
using AspectCore.Injector;
using ConsoleSample.Interceptors;
using WeihanLi.Common;

namespace ConsoleSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Init();

            var service = DependencyResolver.Current.GetService<ICustomService>();

            service.Call();

            service.Call();// this will not be called

            (service as CustomService)?.Call1();
            (service as CustomService)?.Call2();

            var service1 = DependencyResolver.Current.GetService<CustomService>();
            service1.Call();
            service1.Call1();
            service1.Call2();

            Console.ReadLine();
        }

        private static void Init()
        {
            var container = new ServiceContainer();
            container.AddType<CustomService>();
            container.AddType<ILogger, ConsoleLogger>();
            container.AddType<ICustomService, CustomService>();

            DependencyResolver.SetDependencyResolver(container.Build());

            var interceptorCollection = new InterceptorCollection();

            interceptorCollection.AddTyped<CustomInterceptorAttribute>();
        }
    }

    public interface ILogger
    {
        void Info(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}
