using System;

namespace AopSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var testService = ProxyGenerator.Instance.CreateInterfaceProxy<ITestService>();
            testService.Test();

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
