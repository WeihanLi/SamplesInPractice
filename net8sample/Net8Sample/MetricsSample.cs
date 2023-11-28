// https://github.com/dotnet/runtime/issues/77514 
// https://github.com/dotnet/runtime/pull/86567
// https://github.com/dotnet/runtime/pull/90201

using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using WeihanLi.Extensions;

namespace Net8Sample;

public static class MetricsSample
{
    public static void MainTest()
    {
        // sample for customize Activity.TraceIdGenerator
        // var guid = Guid.NewGuid().ToString("N");
        // Console.WriteLine(guid);
        //
        // Activity.TraceIdGenerator = () => ActivityTraceId.CreateFromString(guid);
        // var activity = new Activity("test");
        // activity.Start();
        // Console.WriteLine(activity.Id);
        //

        var services = new ServiceCollection();
        services.AddMetrics();
        using var serviceProvider = services.BuildServiceProvider();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(nameof(MetricsSample))
            .AddConsoleExporter()
            .Build();
        
        var meterFactory = serviceProvider.GetRequiredService<IMeterFactory>();
        var meter = meterFactory.Create(nameof(MetricsSample));
        // https://learn.microsoft.com/dotnet/core/diagnostics/metrics-instrumentation?WT.mc_id=DT-MVP-5004222#types-of-instruments
        var counter = meter.CreateCounter<int>("test-counter", null, "test counter", GetMetricTags());
        // https://learn.microsoft.com/dotnet/core/diagnostics/metrics-collection?WT.mc_id=DT-MVP-5004222#create-a-custom-collection-tool-using-the-net--api
        var observableGauge = meter.CreateObservableGauge("test-o-gauge", () => DateTimeOffset.UtcNow.ToUnixTimeSeconds(), null, null, GetMetricTags());
        // using var meterListener = new MeterListener();
        // meterListener.InstrumentPublished = (instrument, listener) =>
        // {
        //     if (instrument.Meter.Name is nameof(MetricsSample))
        //     {
        //         listener.EnableMeasurementEvents(instrument);
        //         Console.WriteLine("{0} {1}", nameof(meterListener.InstrumentPublished), JsonSerializer.Serialize(instrument));
        //     }
        // };
        // meterListener.MeasurementsCompleted = (instrument, obj) =>
        // {
        //     Console.WriteLine("{0} {1}", nameof(meterListener.MeasurementsCompleted), JsonSerializer.Serialize(instrument));
        // };
        // meterListener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
        // meterListener.SetMeasurementEventCallback<long>(OnMeasurementRecorded);
        // meterListener.Start();

        while (!Console.KeyAvailable)
        {
            Thread.Sleep(1000);
            counter.Add(Random.Shared.Next(100), new KeyValuePair<string, object?>("host", Environment.MachineName));
            
            // meterListener.RecordObservableInstruments();
        }

        Console.ReadLine();
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
    private static IEnumerable<KeyValuePair<string, object?>> GetMetricTags()
    {
        yield return new KeyValuePair<string, object?>("machine_name", Environment.MachineName);
        yield return new KeyValuePair<string, object?>("pid", Environment.ProcessId);
    }
}
