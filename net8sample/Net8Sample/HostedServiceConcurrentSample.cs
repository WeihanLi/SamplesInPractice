using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Net8Sample;

public static class HostedServiceConcurrentSample
{
    public static async Task MainTest(CancellationToken cancellationToken)
    {
        var hostBuilder = Host.CreateEmptyApplicationBuilder(null);
        hostBuilder.ConfigureHostOptions(x =>
        {
            x.ServicesStartConcurrently = true;
            x.ServicesStopConcurrently = true;
            x.StartupTimeout = TimeSpan.FromMilliseconds(100);
        });
        hostBuilder.Services.AddHostedService<DelayService>();
        hostBuilder.Services.AddHostedService<Delay2Service>();
        hostBuilder.Services.AddHostedService<ReportTimeService>();
        var host = hostBuilder.Build();
        await host.RunAsync(cancellationToken);
    }
    
    public static IHostApplicationBuilder ConfigureHostOptions(this IHostApplicationBuilder hostBuilder, Action<HostOptions> configureOptions)
    {
        hostBuilder.Services.Configure(configureOptions);
        return hostBuilder;
    }

    public static IHostApplicationBuilder ConfigureHostOptions(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, HostOptions> configureOptions)
    {
        hostBuilder.Services.Configure<HostOptions>(options => configureOptions(hostBuilder, options));
        return hostBuilder;
    }
}

file sealed class ReportTimeService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            Console.WriteLine(DateTimeOffset.Now);
        }
    }
}

file class DelayService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(60), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

file sealed class Delay2Service : DelayService
{
}
