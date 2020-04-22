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

    public class TestService : ITestService
    {
        public void Test()
        {
            Console.WriteLine("test invoked");
        }

        public virtual void Test1(int a, string b)
        {
            Console.WriteLine($"a:{a}, b:{b}");
        }

        [TryInvoke1Aspect]
        [TryInvoke2Aspect]
        public virtual string Test2()
        {
            return "Hello";
        }

        [TryInvokeAspect]
        public virtual int Test3()
        {
            return 1;
        }
    }
}
