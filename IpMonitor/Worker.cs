using System.Net;
using WeihanLi.Extensions;

namespace IpMonitor;

public class Worker : BackgroundService
{
    private readonly TimeSpan _period;
    private readonly INotification _notification;
    private readonly ILogger<Worker> _logger;

    private volatile string _previousIpInfo = string.Empty;

    public Worker(IConfiguration configuration, INotification notification, ILogger<Worker> logger)
    {
        _notification = notification;
        _logger = logger;
        _period = configuration.GetAppSetting<TimeSpan>("MonitorPeriod");
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
                var ipInfo = ips.Order(new IpAddressComparer()).Select(x => x.MapToIPv4().ToString()).StringJoin(", ");
                if (_previousIpInfo == ipInfo)
                {
                    _logger.LogDebug("IpInfo not changed");
                    continue;
                }

                _logger.LogInformation("Ip info: {IpInfo}", ipInfo);
                await _notification.SendNotification(ipInfo);
                _previousIpInfo = ipInfo;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetIp exception");
            }
        }
    }
}
