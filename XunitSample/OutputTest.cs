using System;
using Xunit;
using Xunit.Abstractions;

namespace XunitSample
{
    public class OutputTest
    {
        private readonly ITestOutputHelper _outputHelper;

        public OutputTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void ConsoleWriteTest()
        {
            Console.WriteLine("Console");
        }

        [Fact]
        public void OutputHelperTest()
        {
            _outputHelper.WriteLine("Output");
        }
    }
}
