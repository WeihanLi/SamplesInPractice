using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;

namespace Net8Sample;

public static class HostedLifecycleServiceSample
{
    public static async Task MainTest(CancellationToken cancellationToken)
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.Services.AddHostedService<MyHostedLifecycleService>();
        var host = hostBuilder.Build();
        await host.RunAsync(cancellationToken);
    }
}


file sealed class MyHostedLifecycleService : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => ReportStatus();

    public Task StopAsync(CancellationToken cancellationToken) => ReportStatus();

    public Task StartedAsync(CancellationToken cancellationToken) => ReportStatus();

    public Task StartingAsync(CancellationToken cancellationToken) => ReportStatus();

    public Task StoppedAsync(CancellationToken cancellationToken) => ReportStatus();

    public Task StoppingAsync(CancellationToken cancellationToken) => ReportStatus();
    
    private static Task ReportStatus([CallerMemberName] string? methodName = default)
    {
        Console.WriteLine($"{nameof(MyHostedLifecycleService)} {methodName}");
        return Task.CompletedTask;
    }
}
