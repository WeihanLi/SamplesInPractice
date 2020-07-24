using AspectCore.DynamicProxy;
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

        private readonly IService _service;

        public CustomServiceTest(IIdGenerator idGenerator, ITestOutputHelper outputHelper, CustomService customService, IService service)
        {
            _idGenerator = idGenerator;
            _outputHelper = outputHelper;
            _customService = customService;
            _service = service;
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

        [Fact]
        public void AspectCoreTest()
        {
            var val = _service.GetValue();
            Assert.Equal("proxy", val);
            Assert.True(_service.IsProxy());
        }
    }
}
