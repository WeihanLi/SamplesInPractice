using System;
using System.Threading.Tasks;
using Xunit;

namespace XunitSample
{
    public class BasicTest
    {
        [Fact]
        public void AddTest()
        {
            Assert.Equal(4, Helper.Add(2, 2));
            Assert.NotEqual(3, Helper.Add(2, 2));
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        public void AddTestWithTestData(int num1, int num2)
        {
            Assert.Equal(num1 + num2, Helper.Add(num1, num2));
        }

        [Fact]
        public void ExceptionTest()
        {
            var exceptionType = typeof(ArgumentException);
            Assert.Throws(exceptionType, Helper.ArgumentExceptionTest);
            Assert.Throws<ArgumentException>(testCode: Helper.ArgumentExceptionTest);
        }

        [Fact]
        public void ExceptionAnyTest()
        {
            Assert.Throws<ArgumentNullException>(Helper.ArgumentNullExceptionTest);
            Assert.ThrowsAny<ArgumentNullException>(Helper.ArgumentNullExceptionTest);
            Assert.ThrowsAny<ArgumentException>(Helper.ArgumentNullExceptionTest);
        }
        
        [Fact]
        public async Task ConcurrencyTest()
        {
            Assert.False(Helper.Ready);
            await Task.Run(Helper.MarkReady, TestContext.Current.CancellationToken);
            Parallel.For(1, 1_000_000, i =>
            {
                Assert.True(Helper.Ready);
            });
        }
    }
}
