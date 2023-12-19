using System.Collections.Frozen;

namespace IpMonitor;

public interface INotificationSelector
{
    INotification SelectNotification(string type);
}

public sealed class NotificationSelector : INotificationSelector
{
    private readonly FrozenDictionary<string, INotification> _notifications;

    public NotificationSelector(IEnumerable<INotification> notifications)
    {
        _notifications = notifications.ToFrozenDictionary(
            x => x.NotificationType, x => x, StringComparer.OrdinalIgnoreCase
        );
    }
    
    public INotification SelectNotification(string type)
    {
        if (_notifications.TryGetValue(type, out var notification))
            return notification;

        throw new InvalidOperationException($"Invalid notification type: [{type}]");
    }
}
