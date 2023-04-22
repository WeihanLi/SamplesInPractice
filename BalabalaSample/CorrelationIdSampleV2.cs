using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using WeihanLi.Common.Logging.Serilog;
using WeihanLi.Extensions;

namespace BalabalaSample;

public static class CorrelationIdSampleV2
{
    public static async Task MainTest()
    {
        // using var activityListener = new ActivityListener();
        // activityListener.ShouldListenTo = _ => true;
        // activityListener.ActivityStarted = activity =>
        // {
        //     Console.WriteLine($"activity {activity.DisplayName} started, activityId: {activity.Id}, traceId: {activity.TraceId}");
        // };
        // activityListener.ActivityStopped = activity =>
        // {
        //     Console.WriteLine($"activity {activity.DisplayName} stopped, activityId: {activity.Id}, traceId: {activity.TraceId}");
        // };
        // activityListener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData;
        // ActivitySource.AddActivityListener(activityListener);
        // Console.WriteLine(ServiceScopeExtensions.ActivitySource.HasListeners());
        // Console.ReadLine();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(x => x.AddService("BalabalaSample"))
            // .AddSource("System.*")
            // .AddSource("Microsoft.*")
            .AddSource(ServiceScopeExtensions.ActivitySource.Name)
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .Build();

        SerilogHelper.LogInit(configuration =>
        {
            configuration.Enrich.With<ActivityEnricher>();
            configuration.WriteTo.Console(LogEventLevel.Information
                , "[{Timestamp:HH:mm:ss} {Level:u3}] {TraceId} {SpanId} {Message:lj}{NewLine}{Exception}"
            );
        });

        var serviceCollection = new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog());
        serviceCollection.AddHttpClient("test",
            client => { client.BaseAddress = new Uri("https://reservation.weihanli.xyz"); });
        await using var provider = serviceCollection.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(CorrelationIdSampleV2));
        logger.LogInformation("Hello 1234");
        provider.ExecuteWithCorrelationScope((_, _) =>
        {
            logger.LogInformation("Correlation 1-1");
            // do something
            Thread.Sleep(100);
            logger.LogInformation("Correlation 1-2");
        });

        await provider.ExecuteWithCorrelationScopeAsync(async (scope, __) =>
        {
            logger.LogInformation("Correlation 2-1");

            await Task.Delay(100);

            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("test");
            using var response = await httpClient.GetAsync("/health");
            Console.WriteLine($"HttpRequestHeaders: {
                response.RequestMessage?.Headers.Select(_ => $"{{{{{_.Key}: {_.Value.StringJoin(",")}}}").StringJoin(Environment.NewLine)
            }");
            var responseText = await response.Content.ReadAsStringAsync();
            logger.LogInformation("ApiResponse: {responseStatus} {responseText}", response.StatusCode.ToString(),
                responseText);
            Console.WriteLine();

            logger.LogInformation("Correlation 2-2");
        });

        logger.LogInformation("Hello 4567");
    }
}

file static class ServiceScopeExtensions
{
    public static readonly ActivitySource ActivitySource = new ActivitySource("ActivitySample");

    public static void ExecuteWithCorrelationScope(this IServiceProvider serviceProvider,
        Action<IServiceScope, string> action)
    {
        var scope = serviceProvider.CreateScope();
        var activity = ActivitySource.StartActivity();
        try
        {
            action.Invoke(scope, activity?.TraceId.ToString() ?? Guid.NewGuid().ToString());
        }
        finally
        {
            activity?.Stop();
            scope.Dispose();
        }
    }

    public static async Task ExecuteWithCorrelationScopeAsync(this IServiceProvider serviceProvider,
        Func<IServiceScope, string, Task> action)
    {
        var scope = serviceProvider.CreateScope();
        var activity = ActivitySource.StartActivity();
        try
        {
            await action.Invoke(scope, activity?.TraceId.ToString() ?? Guid.NewGuid().ToString());
        }
        finally
        {
            activity?.Stop();
            scope.Dispose();
        }
    }
}

file sealed class ActivityEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (Activity.Current != null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(
                    nameof(Activity.TraceId),
                    Activity.Current.TraceId)
            );
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(
                    nameof(Activity.SpanId),
                    Activity.Current.SpanId)
            );
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(
                    "ActivityId",
                    Activity.Current.Id)
            );
        }
    }
}
