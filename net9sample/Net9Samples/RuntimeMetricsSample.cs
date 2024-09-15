using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Net9Samples;

public static class RuntimeMetricsSample
{
    public static async Task RunAsync()
    {
        using var _ = Sdk.CreateMeterProviderBuilder()
            .AddMeter("System.Runtime")
            .AddConsoleExporter()
            .Build();

        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            GC.Collect();
        }
    }
}
