using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeihanLi.Common.Services;

namespace Net7Sample;

internal class HostApplicationBuilderSample
{
    public static async Task MainTest()
    {
        var builder = Host.CreateApplicationBuilder();
                
        builder.Logging.AddJsonConsole(config =>
        {
            config.UseUtcTimestamp = true;
            config.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
            config.JsonWriterOptions = new System.Text.Json.JsonWriterOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = true
            };
        });

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
