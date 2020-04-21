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
    }
}
