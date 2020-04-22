using System;

namespace AopSample
{
    public interface ITestService
    {
        [TryInvokeAspect]
        void Test();

        [TryInvokeAspect]
        [TryInvoke1Aspect]
        [TryInvoke2Aspect]
        void Test1(int a, string b);

        [TryInvokeAspect]
        string Test2();

        [TryInvokeAspect]
        int Test3();
    }

    internal class TestService : ITestService
    {
        public void Test()
        {
        }

        public void Test1(int a, string b)
        {
            Console.WriteLine($"a:{a}, b:{b}");
        }

        public string Test2()
        {
            return "Hello";
        }

        public int Test3()
        {
            return 1;
        }
    }
}
