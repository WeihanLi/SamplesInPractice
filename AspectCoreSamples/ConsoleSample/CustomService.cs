using System;
using ConsoleSample.Interceptors;

namespace ConsoleSample
{
    public interface ICustomService
    {
        [CustomInterceptor]
        void Call();
    }

    public class CustomService : ICustomService
    {
        public void Call()
        {
            Console.WriteLine("service calling...");
        }

        [CustomInterceptor]
        public void Call1()
        {
            Console.WriteLine("service call1");
        }

        [CustomInterceptor]
        public virtual void Call2()
        {
            Console.WriteLine("service call2");
        }
    }
}
