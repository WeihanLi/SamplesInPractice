using System.Net;
using WeihanLi.Extensions;

namespace IpMonitor;

public sealed class Worker : BackgroundService
{
    private readonly TimeSpan _period;
    private readonly INotification _notification;
    private readonly ILogger<Worker> _logger;

    private volatile string _previousIpInfo = string.Empty;

    public Worker(IConfiguration configuration, INotificationSelector notificationSelector, ILogger<Worker> logger)
    {
        _logger = logger;
        _period = configuration.GetAppSetting<TimeSpan>("MonitorPeriod");
        _notification = notificationSelector.SelectNotification(configuration.GetAppSetting("NotificationType") ?? string.Empty);
        if (_period <= TimeSpan.Zero)
        {
            _period = TimeSpan.FromMinutes(10);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var host = Dns.GetHostName();
                var ips = await Dns.GetHostAddressesAsync(host, stoppingToken);
                var ipInfo = $"{Environment.MachineName} - {host}\n {ips.Order(new IpAddressComparer())
                    .Select(x => x.MapToIPv4().ToString()).StringJoin(", ")}";
                if (_previousIpInfo == ipInfo)
                {
                    _logger.LogDebug("IpInfo not changed");
                    continue;
                }

                _logger.LogInformation("Ip info: {IpInfo}", ipInfo);
                await _notification.SendNotification($"[IpMonitor]\n{ipInfo}");
                _previousIpInfo = ipInfo;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetIp exception");
            }
        }
    }
}
