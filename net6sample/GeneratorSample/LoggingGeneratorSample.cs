using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GeneratorSample;

public partial class LoggingGeneratorSample
{
    public static void MainTest()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions()
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }));

        var logger = loggerFactory.CreateLogger<LoggingGeneratorSample>();
        logger.TestBegin();
        logger.TestWithArgument(LogLevel.Warning, Environment.UserName);
        logger.TestWithEmptyMessage(LogLevel.Information, Environment.MachineName);
        logger.TestEnd();

        // instance logging test
        new InstanceLoggingGenerator(logger).LoggingTest();
    }
}


internal partial class InstanceLoggingGenerator
{
    private readonly ILogger _logger;

    public InstanceLoggingGenerator(ILogger logger)
    {
        _logger = logger;
    }

    [LoggerMessage(EventId = 0, EventName = "Test", Level = LogLevel.Information, Message = "Instance logging generator test")]
    public partial void LoggingTest();
}

public static partial class LoggingHelper
{
    [LoggerMessage(Level = LogLevel.Information, EventId = 0, Message = "Logging generator sample begin")]
    public static partial void TestBegin(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, EventId = 1, Message = "Logging generator sample end")]
    public static partial void TestEnd(this ILogger logger);

    [LoggerMessage(EventId = 2, Message = "Logging generator sample user {userName}")]
    public static partial void TestWithArgument(this ILogger logger, LogLevel logLevel, string userName);

    // warning SYSLIB1015: Argument 'host' is not referenced from the logging message
    [LoggerMessage(EventId = 3)]
    public static partial void TestWithEmptyMessage(this ILogger logger, LogLevel logLevel, string host);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Trace,
        Message = "Fixed message",
        EventName = "CustomEventName")]
    public static partial void LogWithCustomEventName(this ILogger logger);
}
