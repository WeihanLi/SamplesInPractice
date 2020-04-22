using System;

namespace AopSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var testService = ProxyGenerator.Instance.CreateInterfaceProxy<ITestService>();
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
