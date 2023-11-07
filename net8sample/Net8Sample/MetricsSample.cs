// https://github.com/dotnet/runtime/issues/77514 
// https://github.com/dotnet/runtime/pull/86567
// https://github.com/dotnet/runtime/pull/90201

using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;
using WeihanLi.Extensions;

namespace Net8Sample;

public static class MetricsSample
{
    public static void MainTest()
    {
        var services = new ServiceCollection();
        services.AddMetrics();

        using var serviceProvider = services.BuildServiceProvider();
        var meterFactory = serviceProvider.GetRequiredService<IMeterFactory>();
        var meter = meterFactory.Create(nameof(MetricsSample));
        // https://learn.microsoft.com/dotnet/core/diagnostics/metrics-instrumentation?WT.mc_id=DT-MVP-5004222#types-of-instruments
        var counter = meter.CreateCounter<int>("test-counter");
        // https://learn.microsoft.com/dotnet/core/diagnostics/metrics-collection?WT.mc_id=DT-MVP-5004222#create-a-custom-collection-tool-using-the-net--api
        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name is nameof(MetricsSample))
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        meterListener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
        meterListener.Start();

        while (!Console.KeyAvailable)
        {
            counter.Add(Random.Shared.Next(100), new KeyValuePair<string, object?>("host", Environment.MachineName));
            Thread.Sleep(500);
        }
    }
    
    private static void OnMeasurementRecorded<T>(
        Instrument instrument,
        T measurement,
        ReadOnlySpan<KeyValuePair<string, object?>> tags,
        object? state)
    {
        var tag = tags.ToArray().Select(x => $$"""{{{x.Key}}: {{x.Value}}}""")
            .StringJoin(";");
        Console.WriteLine($"{instrument.Name} recorded measurement {measurement}, tag: {tag}");
    }
}
