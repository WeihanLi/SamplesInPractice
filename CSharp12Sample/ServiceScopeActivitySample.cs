using System.Diagnostics;

namespace CSharp12Sample;

public static class ServiceScopeActivitySample
{
    public static async Task MainTest()
    {
        using var activityListener = new ActivityListener();
        activityListener.ShouldListenTo = s => s.Name == nameof(ActivityScope);
        activityListener.Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
            ActivitySamplingResult.PropagationData;
        ActivitySource.AddActivityListener(activityListener);
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ActivityScope>();
        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        {
            using var scope = serviceProvider.CreateScope();
            Console.WriteLine("Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
            Console.WriteLine("activity scope activityId:");
            Console.WriteLine(scope.ServiceProvider.GetRequiredService<ActivityScope>().Activity?.Id);
            Console.WriteLine();
            Console.WriteLine("Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
        }
        Console.WriteLine();
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            Console.WriteLine("CreateAsyncScope Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
        }
        Console.WriteLine();
    }
}

public sealed class ActivityScope : IDisposable, IAsyncDisposable
{
    private static readonly ActivitySource ActivitySource = new(nameof(ActivityScope));

    private readonly Activity? _activity = ActivitySource.StartActivity(nameof(ActivityScope));

    public Activity? Activity => _activity;
    
    public void Dispose()
    {
        Console.WriteLine("Dispose ing...");
        _activity?.Dispose();
        Console.WriteLine("Dispose done");
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("DisposeAsync ing...");
        _activity?.Dispose();
        Console.WriteLine("DisposeAsync done");
        return ValueTask.CompletedTask;
    }
}
