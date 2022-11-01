using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using Xunit;

namespace XunitSample;

public class MoqLoggerTest
{
    [Fact]
    public void NullLogger()
    {
        var helper = new Helper(NullLogger<Helper>.Instance);
        helper.LogMessage("Test");
    }

    [Theory]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void VerifyLogMessage(LogLevel logLevel)
    {
        var message = "Test 1234";
        Mock<ILogger<Helper>> loggerMock = new();
        var helper = new Helper(loggerMock.Object);
        helper.LogMessage(message, logLevel);

        loggerMock.VerifyLogWithMessage(message, logLevel);
    }

    [Theory]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void VerifyLogMessagePredict(LogLevel logLevel)
    {
        var message = "Test 1234";
        Mock<ILogger<Helper>> loggerMock = new();
        var helper = new Helper(loggerMock.Object);
        helper.LogMessage(message, logLevel);

        loggerMock.VerifyLogWithMessage(s => s.Contains("Test"), logLevel);
    }

    [Theory]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void VerifyLogMessageWithArguments(LogLevel logLevel)
    {
        var message = "Test {Number}";
        Mock<ILogger<Helper>> loggerMock = new();
        var helper = new Helper(loggerMock.Object);
        helper.LogMessage(message, logLevel, args: new object[] { 1234 });

        loggerMock.VerifyLogWithMessage("Test 1234", logLevel);
    }

    [Theory]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void VerifyLogMessageWithException(LogLevel logLevel)
    {
        var message = "Test 1234";
        Mock<ILogger<Helper>> loggerMock = new();
        var helper = new Helper(loggerMock.Object);
        helper.LogMessage(message, logLevel, new ArgumentException());

        loggerMock.VerifyLogWithMessage(message, logLevel, ex => ex is ArgumentException);
    }


    [Theory]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void VerifyLogException(LogLevel logLevel)
    {
        var message = "Test 1234";
        Mock<ILogger<Helper>> loggerMock = new();
        var helper = new Helper(loggerMock.Object);
        helper.LogMessage(message, logLevel, new ArgumentException());

        loggerMock.VerifyLogWithException(ex => ex is ArgumentException, logLevel);
    }
}
