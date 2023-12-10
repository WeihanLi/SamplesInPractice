using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using WeihanLi.Extensions.Hosting;

namespace TestLibrary;

public sealed class TimeEchoService(IServiceProvider serviceProvider) : TimerBaseBackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    
    protected override Task ExecuteTaskAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine();
        using var scope = _serviceProvider.CreateScope();
        Console.WriteLine($"current time is {DateTimeOffset.Now}, activityId: {Activity.Current?.Id}");
        return Task.CompletedTask;
    }

    protected override TimeSpan Period => TimeSpan.FromSeconds(1);
}
