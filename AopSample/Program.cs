using System;

namespace AopSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var testService = ProxyGenerator.Instance.CreateInterfaceProxy<ITestService>();
            var testService = ProxyGenerator.Instance.CreateInterfaceProxy<ITestService, TestService>();
            //var testService = ProxyGenerator.Instance.CreateClassProxy<TestService>();
            testService.Test();
            Console.WriteLine();
            testService.Test1(1, "str");

            // testService.TestProp = "12133";

            var a = testService.Test2();

            var b = testService.Test3();
            Console.WriteLine($"a:{a}, b:{b}");
            Console.ReadLine();
        }
    }
}
