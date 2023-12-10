using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using TestLibrary;

namespace InterceptorPlayground;

public class ActivityScopeSample
{
    public static async Task MainTest()
    {
        using var activityListener = new ActivityListener();
        activityListener.ShouldListenTo = s => s.Name == nameof(ScopeActivityCreator);
        activityListener.Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
            ActivitySamplingResult.PropagationData;
        ActivitySource.AddActivityListener(activityListener);

        var appBuilder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        appBuilder.Services.AddScoped<ScopeActivityCreator>();
        appBuilder.Services.AddHostedService<TimeEchoService>();
        var app = appBuilder.Build();
        {
            using var scope = app.Services.CreateScope();
            Console.WriteLine("Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
            Console.WriteLine("activity scope activityId:");
            Console.WriteLine(scope.ServiceProvider.GetRequiredService<ScopeActivityCreator>().Activity?.Id);
            Console.WriteLine();
            Console.WriteLine("Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
        }
        Console.WriteLine();
        {
            await using var scope = app.Services.CreateAsyncScope();
            Console.WriteLine("CreateAsyncScope Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
        }
        Console.WriteLine();
        await app.StartAsync();
        Console.ReadLine();
        if (app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (app is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
