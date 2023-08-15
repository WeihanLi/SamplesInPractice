using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using WeihanLi.Common.Logging.Serilog;
using WeihanLi.Extensions;

namespace BalabalaSample;

public static class CorrelationIdSample
{
    public static async Task MainTest()
    {
        SerilogHelper.LogInit(configuration =>
        {
            configuration.Enrich.With<CorrelationEnricher>();
            configuration.WriteTo.Console(LogEventLevel.Information
                , "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"
            );
        });

        var serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog())
                .AddTransient<CorrelationHttpHandler>()
            ;
        serviceCollection.AddHttpClient("test",
                client => { client.BaseAddress = new Uri("https://reservation.weihanli.xyz"); })
            .AddHttpMessageHandler<CorrelationHttpHandler>();
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

            await Task.Delay(100);

            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("test");
            using var response = await httpClient.GetAsync("/health");
            var httpRequestId = string.Empty;
            if (response.RequestMessage?.Headers.Contains(CorrelationHttpHandler.RequestIdHeaderName) == true)
            {
                httpRequestId =
                    response.RequestMessage.Headers.GetValues(CorrelationHttpHandler.RequestIdHeaderName)
                        .StringJoin(",");
            }
            Console.WriteLine($"HttpRequestId: {httpRequestId}");
            var responseText = await response.Content.ReadAsStringAsync();
            logger.LogInformation("ApiResponse: {responseStatus} {responseText}", response.StatusCode.ToString(),
                responseText);

            logger.LogInformation("Correlation 2-2");
        });

        logger.LogInformation("Hello 4567");
    }
}

file static class ServiceScopeExtensions
{
    public static void ExecuteWithCorrelationScope(this IServiceProvider serviceProvider,
        Action<IServiceScope, string> action)
    {
        var scope = serviceProvider.CreateScope();
        try
        {
            var correlationContext = new CorrelationContext();
            CorrelationContextAccessor.Context = correlationContext;
            action.Invoke(scope, correlationContext.CorrelationId);
        }
        finally
        {
            CorrelationContextAccessor.Context = null;
            scope.Dispose();
        }
    }

    public static async Task ExecuteWithCorrelationScopeAsync(this IServiceProvider serviceProvider,
        Func<IServiceScope, string, Task> action)
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
    private static readonly AsyncLocal<CorrelationContext?> ContextCurrent = new();

    public static CorrelationContext? Context
    {
        get => ContextCurrent.Value;
        set => ContextCurrent.Value = value;
    }
}

file sealed class CorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> ContextCurrent = new();

    public static string? CorrelationId
    {
        get => ContextCurrent.Value;
        set => ContextCurrent.Value = value;
    }
}

public sealed class CorrelationHttpHandler : DelegatingHandler
{
    public const string RequestIdHeaderName = "x-request-id";
    private const string OriginalRequestIdHeaderName = "x-request-id-original";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
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

file sealed class CorrelationEnricher : ILogEventEnricher
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
