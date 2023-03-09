using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using WeihanLi.Common.Logging.Serilog;

namespace BalabalaSample;

public static class CorrelationIdSample
{
    public static async Task MainTest()
    {
        SerilogHelper.LogInit(configuration =>
        {
            configuration.Enrich.With<CorrelationIdEnricher>();
            configuration.WriteTo.Console(LogEventLevel.Information, "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}");
        });

        var serviceCollection = new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog())
            .AddTransient<CorrelationIdHttpHandler>()
            ;
        serviceCollection.AddHttpClient("test", client =>
            {
                client.BaseAddress = new Uri("https://reservation.weihanli.xyz");
            })
            .AddHttpMessageHandler<CorrelationIdHttpHandler>();
        await using var provider = serviceCollection.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CorrelationIdSample));
        logger.LogInformation("Hello 1234");
        provider.ExecuteWithCorrelationScope((_, _) =>
        {
            logger.LogInformation("Correlation 1-1");
            // do something
            Thread.Sleep(100);
            logger.LogInformation("Correlation 1-2");
        });

        await provider.ExecuteWithCorrelationScopeAsync(async (scope, _) =>
        {
            logger.LogInformation("Correlation 2-1");

            var httpClient = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("test");
            using var response = await httpClient.GetAsync("/health");
            var responseText = await response.Content.ReadAsStringAsync();
            logger.LogInformation("ApiResponse: {responseStatus} {responseText}", response.StatusCode.ToString(), responseText);
            
            logger.LogInformation("Correlation 2-2");
        });
        
        logger.LogInformation("Hello 4567");
    }
}

file static class ServiceScopeExtensions
{
    public static void ExecuteWithCorrelationScope(this IServiceProvider serviceProvider, Action<IServiceScope, string> action)
    {
        var scope = serviceProvider.CreateScope();
        try
        {
            var correlationId = Guid.NewGuid().ToString();
            CorrelationContextAccessor.Context = new CorrelationContext();
            action.Invoke(scope, correlationId);
        }
        finally
        {
            CorrelationContextAccessor.Context = null;
            scope.Dispose();
        }
    }
    
    public static async Task ExecuteWithCorrelationScopeAsync(this IServiceProvider serviceProvider, Func<IServiceScope, string, Task> action)
    {
        var scope = serviceProvider.CreateScope();
        try
        {
            var correlationContext = new CorrelationContext();
            CorrelationContextAccessor.Context = correlationContext;
            await action.Invoke(scope, correlationContext.CorrelationId);
        }
        finally
        {
            CorrelationContextAccessor.Context = null;
            scope.Dispose();
        }
    }
}

file sealed class CorrelationContext
{
    public CorrelationContext()
    {
        CorrelationId = Guid.NewGuid().ToString();
    }
    
    public string CorrelationId { get; }
}
file sealed class CorrelationContextAccessor
{
    public const string Name = "CorrelationId";
    
    private static readonly AsyncLocal<ContextHolder> ContextCurrent = new();
    
    public static CorrelationContext? Context
    {
        get
        {
            return ContextCurrent.Value?.Value;
        }
        set
        {
            var holder = ContextCurrent.Value;
            if (holder != null)
            {
                // Clear current Value trapped in the AsyncLocals, as its done.
                holder.Value = null;
            }
            if (value != null)
            {
                // Use an object indirection to hold the Value in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                ContextCurrent.Value = new ContextHolder
                {
                    Value = value
                };
            }
        }
    }
    
    private sealed class ContextHolder
    {
        public CorrelationContext Value;
    }
}

public sealed class CorrelationIdHttpHandler : DelegatingHandler
{
    private const string RequestIdHeaderName = "x-request-id";
    private const string OriginalRequestIdHeaderName = "x-request-id-original";
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (CorrelationContextAccessor.Context != null)
        {
            var correlationId = CorrelationContextAccessor.Context.CorrelationId;
            if (request.Headers.TryGetValues(RequestIdHeaderName, out var originalRequestId))
            {
                request.Headers.Add(OriginalRequestIdHeaderName, originalRequestId);
                request.Headers.Remove(RequestIdHeaderName);
            }
            request.Headers.Add(RequestIdHeaderName, correlationId);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
file sealed class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationContext = CorrelationContextAccessor.Context;
        if (correlationContext != null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(
                    nameof(CorrelationContext.CorrelationId), 
                    correlationContext.CorrelationId)
                );
        }
    }
}
