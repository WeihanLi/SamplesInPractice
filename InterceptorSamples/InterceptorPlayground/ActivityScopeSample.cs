using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using TestLibrary;
using WeihanLi.Common.Helpers;

namespace InterceptorPlayground;

public static class ActivityScopeSample
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
            Console.WriteLine("=== IServiceProvider.CreateScope() sample ===");
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
            Console.WriteLine("=== IServiceProvider.CreateAsyncScope() sample ===");
            await using var scope = app.Services.CreateAsyncScope();
            Console.WriteLine("CreateAsyncScope Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
        }
        Console.WriteLine();
        {
            Console.WriteLine("=== IServiceScopeFactory.CreateScope() sample ===");
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            Console.WriteLine("Current activityId:");
            Console.WriteLine(Activity.Current?.Id);
        }
        
        ConsoleHelper.ReadLineWithPrompt();
        await app.StartAsync();
        ConsoleHelper.ReadLineWithPrompt("Press Enter to exit");
        await app.StopAsync();
        
        switch (app)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}
