using System;

namespace AspNetCoreSample
{
    public interface ICustomService
    {
        [LogInterceptor]
        void Call();
    }

    public class CustomService : ICustomService
    {
        public void Call()
        {
            Console.WriteLine("service calling...");
        }
    }
}