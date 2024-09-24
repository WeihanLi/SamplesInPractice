using IpMonitor;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.Diagnostics;
using System.Runtime.InteropServices;

const string serviceName = "IpMonitor";

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = args,
        ApplicationName = serviceName,
#if DEBUG
        EnvironmentName = Environments.Development,
#endif
    });

builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true);
    
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<INotification, GoogleChatNotification>();
builder.Services.AddSingleton<INotification, DingBotNotification>();
builder.Services.AddSingleton<INotificationSelector, NotificationSelector>();

AddServices(builder.Services);

await builder.Build().RunAsync();


void AddServices(IServiceCollection services)
{
    if (WindowsServiceHelpers.IsWindowsService())
    {
        services.AddLogging(logging =>
        {
            Debug.Assert(OperatingSystem.IsWindows());
            logging.AddEventLog(x=>
            {
                Debug.Assert(OperatingSystem.IsWindows());
                x.SourceName = serviceName;
            });
        });
        services.AddSingleton<IHostLifetime, WindowsServiceLifetime>();
        services.Configure<WindowsServiceLifetimeOptions>(options =>
        {
            options.ServiceName = serviceName;
        });
    }
    else
    {
        services.AddLogging(logging =>
        {
            logging.AddSimpleConsole();
        });
    }
}

