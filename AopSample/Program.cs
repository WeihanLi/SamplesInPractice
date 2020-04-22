using System;

namespace AopSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var testService0 = ProxyGenerator.Instance.CreateClassProxy<ITestService>();
            //var testService1 = ProxyGenerator.Instance.CreateClassProxy<ITestService, TestService>();
            var testService = ProxyGenerator.Instance.CreateClassProxy<TestService>();
            testService.Test();
            Console.WriteLine();
            testService.Test1(1, "str");

            var a = testService.Test2();

            var b = testService.Test3();
            Console.WriteLine($"a:{a}, b:{b}");
            Console.ReadLine();
        }
    }
}
