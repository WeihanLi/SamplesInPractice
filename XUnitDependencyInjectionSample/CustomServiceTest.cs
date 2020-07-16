using WeihanLi.Common;
using Xunit;
using Xunit.Abstractions;

namespace XUnitDependencyInjectionSample
{
    public class CustomServiceTest
    {
        private readonly IIdGenerator _idGenerator;
        private readonly ITestOutputHelper _outputHelper;
        private readonly CustomService _customService;

        public CustomServiceTest(IIdGenerator idGenerator, ITestOutputHelper outputHelper, CustomService customService)
        {
            _idGenerator = idGenerator;
            _outputHelper = outputHelper;
            _customService = customService;
        }

        [Fact]
        public void NewIdTest()
        {
            var newId = _idGenerator.NewId();
            Assert.NotNull(newId);
            _outputHelper.WriteLine(newId);
        }

        [Fact]
        public void TestOutputHelperAccessorTest()
        {
            _customService.Output("Hello World");
        }
    }
}
