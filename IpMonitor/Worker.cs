using System.Net;
using WeihanLi.Extensions;
using WeihanLi.Extensions.Hosting;

namespace IpMonitor;

public sealed class Worker : TimerBaseBackgroundServiceWithDiagnostic
{
    private readonly TimeSpan _period;
    private readonly INotification _notification;
    private readonly ILogger<Worker> _logger;

    private volatile string _previousIpInfo = string.Empty;

    public Worker(IConfiguration configuration, INotificationSelector notificationSelector, ILogger<Worker> logger, IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
        _logger = logger;
        _period = configuration.GetAppSetting<TimeSpan>("MonitorPeriod");
        _notification = notificationSelector.SelectNotification(configuration.GetAppSetting("NotificationType") ?? string.Empty);
        if (_period <= TimeSpan.Zero)
        {
            _period = TimeSpan.FromMinutes(10);
        }
    }

    protected override TimeSpan Period => _period;

    protected override async Task ExecuteTaskInternalAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var host = Dns.GetHostName();
        var ips = await Dns.GetHostAddressesAsync(host, cancellationToken);
        var ipInfo = $"{Environment.MachineName} - {host}\n {ips.Order(new IpAddressComparer())
            .Select(x => x.MapToIPv4().ToString()).StringJoin(", ")}";
        if (_previousIpInfo == ipInfo)
        {
            _logger.LogDebug("IpInfo not changed");
            return;
        }

        _logger.LogInformation("Ip info: {IpInfo}", ipInfo);
        await _notification.SendNotification($"[IpMonitor]\n{ipInfo}");
        _previousIpInfo = ipInfo;
    }
}
