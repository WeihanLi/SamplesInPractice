using System.Threading;
using Xunit;

namespace XunitSample
{
    [UseCulture("en-US", "zh-CN")]
    public class FilterTest
    {
        [Fact]
        [UseCulture("en-US")]
        public void CultureTest()
        {
            Assert.Equal("en-US", Thread.CurrentThread.CurrentCulture.Name);
        }

        [Fact]
        [UseCulture("zh-CN")]
        public void CultureTest2()
        {
            Assert.Equal("zh-CN", Thread.CurrentThread.CurrentCulture.Name);
        }

        [Fact]
        public void CultureTest3()
        {
            Assert.Equal("en-US", Thread.CurrentThread.CurrentCulture.Name);
            Assert.Equal("zh-CN", Thread.CurrentThread.CurrentUICulture.Name);
        }
    }
}
