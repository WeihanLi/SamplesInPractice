using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using WeihanLi.Extensions;
using WeihanLi.Extensions.Hosting;

namespace IpMonitor;

public sealed class Worker(
    IConfiguration configuration, 
    INotificationSelector notificationSelector, 
    ILogger<Worker> logger, 
    IServiceProvider serviceProvider
    ) 
    : TimerBaseBackgroundServiceWithDiagnostic(serviceProvider)
{
    private readonly bool _ipV4Only = configuration.GetAppSetting("IPV4Only").ToBoolean(true);
    private readonly INotification _notification =
        notificationSelector.SelectNotification(configuration.GetRequiredAppSetting("NotificationType"));
    private volatile string _previousIpInfo = string.Empty;

    protected override TimeSpan Period { get; } =
        TimeSpan.TryParse(configuration["AppSettings:MonitorPeriod"], out var period) && period > TimeSpan.Zero
            ? period : TimeSpan.FromSeconds(10);

    protected override async Task ExecuteTaskInternalAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var ipList = new List<IPAddress>();
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var ipProperties = networkInterface.GetIPProperties();
            foreach (var ipAddress in ipProperties.UnicastAddresses)
            {
                logger.LogInformation("Interface: {InterfaceName}, type: {InterfaceType}, IP: {IPAddress}, AddressFamily: {AddressFamily}",
                    networkInterface.Name, networkInterface.NetworkInterfaceType.ToString(), ipAddress.Address, ipAddress.Address.AddressFamily
                );
                
                if (_ipV4Only && ipAddress.Address.AddressFamily is not AddressFamily.InterNetwork)
                    continue;
                
                ipList.Add(ipAddress.Address);
            }
        }
        
        var ipInfo = $"{Environment.MachineName} \n {
            ipList.Order(new IpAddressComparer())
            .Select(x => x.ToString()).StringJoin(", ")
        }";
        if (_previousIpInfo == ipInfo)
        {
            logger.LogDebug("IpInfo not changed");
            return;
        }

        logger.LogInformation("Ip info: {IpInfo}", ipInfo);
        await _notification.SendNotification($"[IpMonitor]\n{ipInfo}");
        _previousIpInfo = ipInfo;
    }
}
