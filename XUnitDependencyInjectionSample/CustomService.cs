using Xunit.DependencyInjection;

namespace XUnitDependencyInjectionSample
{
    public class CustomService
    {
        private readonly ITestOutputHelperAccessor _testOutputHelperAccessor;

        public CustomService(ITestOutputHelperAccessor testOutputHelperAccessor)
        {
            _testOutputHelperAccessor = testOutputHelperAccessor;
        }

        public void Output(string output)
        {
            _testOutputHelperAccessor.Output?.WriteLine(output);
        }
    }
}
