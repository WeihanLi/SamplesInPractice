using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeihanLi.Common.Services;

namespace Net7Sample;

internal class HostApplicationBuilderSample
{
    public static async Task MainTest()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddJsonFile("env.json", true);
        builder.Services.AddSingleton<IUserIdProvider>(EnvironmentUserIdProvider.Instance.Value);
        builder.Services.AddHostedService<TestHostedService>();
        var host = builder.Build();
        await host.StartAsync();
    }

    private sealed class TestHostedService : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                Console.WriteLine($"{DateTime.Now}");
            }
        }
    }
}
