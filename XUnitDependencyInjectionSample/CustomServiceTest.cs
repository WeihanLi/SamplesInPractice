using WeihanLi.Common;
using Xunit;
using Xunit.Abstractions;

namespace XUnitDependencyInjectionSample
{
    public class CustomServiceTest
    {
        private readonly IIdGenerator _idGenerator;
        private readonly ITestOutputHelper _outputHelper;

        public CustomServiceTest(IIdGenerator idGenerator, ITestOutputHelper outputHelper)
        {
            _idGenerator = idGenerator;
            _outputHelper = outputHelper;
        }

        [Fact]
        public void NewIdTest()
        {
            var newId = _idGenerator.NewId();
            Assert.NotNull(newId);
            _outputHelper.WriteLine(newId);
        }
    }
}
