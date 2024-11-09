using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Diagnostics.Metrics;

namespace Net9Samples;

public static class MetricsSample
{
    public static async Task RunAsync()
    {
        using var _ = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Net9Samples"))
            .AddMeter(nameof(MetricsSample))
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 500;
                options.BatchExportProcessorOptions.MaxQueueSize = 1;
            })
            .Build();
        
        var meter = new Meter(nameof(MetricsSample));
        var maxTimestamp = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeMilliseconds();
        var gauge = meter.CreateGauge<long>("offset");
        var counter = meter.CreateUpDownCounter<long>("counter");
        
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        while (ts <= maxTimestamp)
        {
            counter.Add(1);
            gauge.Record(ts);
            Console.WriteLine($"ts = {ts}");
            await Task.Delay(1000);
            ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        Console.WriteLine("Done");
    }
}
