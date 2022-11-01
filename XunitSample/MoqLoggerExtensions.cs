using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq.Expressions;

namespace XunitSample;

public static class MoqLoggerExtensions
{
    public static void VerifyLogWithException<TLogger>(this Mock<TLogger> loggerMock,
        Expression<Func<Exception, bool>> exceptionPredict, LogLevel logLevel = LogLevel.Error) where TLogger: class, ILogger
    {
        loggerMock.Verify(s => s.Log(logLevel, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.Is(exceptionPredict), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }
    
    public static void VerifyLogWithMessage<TLogger>(this Mock<TLogger> loggerMock,
        string message,
        LogLevel logLevel = LogLevel.Information,
        Expression<Func<Exception, bool>> exceptionPredict = null) where TLogger: class, ILogger
    {
        if (exceptionPredict is null)
        {
            loggerMock.Verify(s => s.Log(logLevel, It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((x,_) => x.ToString().Equals(message)),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));   
        }
        else
        {
            loggerMock.Verify(s => s.Log(logLevel, It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((x,_) => x.ToString().Equals(message)),
                It.Is(exceptionPredict), It.IsAny<Func<It.IsAnyType, Exception, string>>()));            
        }
    }
    
    public static void VerifyLogWithMessage<TLogger>(this Mock<TLogger> loggerMock,
        Func<string, bool> messagePredict,
        LogLevel logLevel = LogLevel.Information,
        Expression<Func<Exception, bool>> exceptionPredict = null) where TLogger: class, ILogger
    {
        if (exceptionPredict is null)
        {
            loggerMock.Verify(s => s.Log(logLevel, It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((x,_) => messagePredict(x.ToString())),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));   
        }
        else
        {
            loggerMock.Verify(s => s.Log(logLevel, It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((x,_) => messagePredict(x.ToString())),
                It.Is(exceptionPredict), It.IsAny<Func<It.IsAnyType, Exception, string>>()));            
        }
    }
}
