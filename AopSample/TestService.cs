namespace AopSample
{
    public interface ITestService
    {
        [TryInvokeAspect]
        void Test();
    }
}
