using IpMonitor;

#if !DEBUG
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
#endif

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
builder.Services.AddSingleton<GoogleChatNotification>();
builder.Services.AddSingleton<DingTalkNotification>();
builder.Services.AddSingleton<INotificationSelector, NotificationSelector>();

AddWindowsLifetime(builder.Services);

await builder.Build().RunAsync();

void AddWindowsLifetime(IServiceCollection services)
{
#if !DEBUG

    if (WindowsServiceHelpers.IsWindowsService())
    {
        services.AddLogging(logging =>
        {
            Debug.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            logging.AddEventLog(x=>
            {
                Debug.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
                x.SourceName = serviceName;
            });
        });
        services.AddSingleton<IHostLifetime, WindowsServiceLifetime>();
        services.Configure<WindowsServiceLifetimeOptions>(options =>
        {
            options.ServiceName = serviceName;
        });
    }  
#endif
}
