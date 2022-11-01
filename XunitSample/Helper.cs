using Microsoft.Extensions.Logging;
using System;

namespace XunitSample
{
    public class Helper
    {
        private readonly ILogger<Helper> _logger;

        public Helper(ILogger<Helper> logger)
        {
            _logger = logger;
        }

        public void LogMessage(string message, LogLevel logLevel = LogLevel.Information, Exception exception = null, object[] args = null)
        {
            _logger.Log(logLevel, exception, message, args ?? Array.Empty<object>());
        }
        
        public static int Add(int x, int y)
        {
            return x + y;
        }

        public static void ArgumentExceptionTest() => throw new ArgumentException();

        public static void ArgumentNullExceptionTest() => throw new ArgumentNullException();
    }
}
