using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;
using WeihanLi.Common.Helpers;

namespace BalabalaSample;

public static class CorrelationIdSampleV3
{
    public static void MainTest()
    {
        var activitySource = new ActivitySource("test");
        using var activityListener = new ActivityListener();
        activityListener.ShouldListenTo = s => s.Name == activitySource.Name;
        activityListener.ActivityStarted = activity =>
        {
            Console.WriteLine($"activity {activity.DisplayName} started, activityId: {activity.Id}, traceId: {activity.TraceId}");
        };
        activityListener.ActivityStopped = activity =>
        {
            Console.WriteLine($"activity {activity.DisplayName} stopped, activityId: {activity.Id}, traceId: {activity.TraceId}");
        };
        activityListener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => 
            Random.Shared.Next(100) < 90 ? ActivitySamplingResult.None : ActivitySamplingResult.AllData;
        
        // activityListener.Sample = (ref ActivityCreationOptions<ActivityContext> options) => 
        //     options.Source.Name == activitySource.Name ? ActivitySamplingResult.AllData : ActivitySamplingResult.None;
        //
        // activityListener.Sample = (ref ActivityCreationOptions<ActivityContext> options) => options.Source.Name == activitySource.Name
        //     ? Random.Shared.Next(100) < 90 ? ActivitySamplingResult.AllData : ActivitySamplingResult.AllDataAndRecorded
        //     : ActivitySamplingResult.None;
        
        ActivitySource.AddActivityListener(activityListener);
        
        // OpenTelemetry
        // using var tracerProvider = Sdk.CreateTracerProviderBuilder()
        //     //.AddSource(activitySource.Name)
        //     .SetSampler(new TraceIdRatioBasedSampler(0.01))
        //     // .AddConsoleExporter()
        //     .Build();

        for (var i = 0; i < 100; i++)
        {
            using var activity = activitySource.StartActivity();
            if (activity is null)
            {
                Console.WriteLine("activity is null");
            }
            
            // using var activity = activitySource.CreateActivity(nameof(MainTest), ActivityKind.Internal);
            // if (activity is null)
            // {
            //     Console.WriteLine("activity is null");
            // }
            
            // https://github.com/dotnet/aspnetcore/blob/main/src/Hosting/Hosting/src/Internal/HostingApplicationDiagnostics.cs#L274
            // using var activity = activitySource.CreateActivity(nameof(MainTest), ActivityKind.Internal)
            //     ?? new Activity(nameof(MainTest));
            // activity.Start();
            // Console.WriteLine($"new activity created {activity.Source.Name}, {activity.Id}");
        }

        ConsoleHelper.ReadLineWithPrompt();
    }
}
