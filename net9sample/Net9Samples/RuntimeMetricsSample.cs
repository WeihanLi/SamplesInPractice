using OpenTelemetry;
using OpenTelemetry.Metrics;
using WeihanLi.Common.Helpers;

namespace Net9Samples;

public static class RuntimeMetricsSample
{
    public static async Task RunAsync()
    {
        await ConsoleSample();
    }

    private static async Task ConsoleSample()
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
     
        // ReSharper disable once FunctionNeverReturns
    }
}
