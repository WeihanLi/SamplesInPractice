using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace IpMonitor;

public interface INotification
{
    string NotificationType { get; }
    Task<bool> SendNotification(string text);
}

public interface INotificationSelector
{
    INotification SelectNotification(string type);
}

public sealed class GoogleChatNotification: INotification
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public GoogleChatNotification(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = Guard.NotNullOrEmpty(configuration.GetAppSetting("WebhookUrl"));
    }
    public string NotificationType => "GoogleChat";
    public async Task<bool> SendNotification(string text)
    {
        // https://developers.google.com/chat/api/guides/message-formats/basic
        using var response = await _httpClient.PostAsJsonAsync(_webhookUrl, new { text });
        return response.IsSuccessStatusCode;
    }
}

public sealed class DingTalkNotification : INotification
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public DingTalkNotification(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = Guard.NotNullOrEmpty(configuration.GetAppSetting("WebhookUrl"));
    }

    public string NotificationType => "DingBot";

    public async Task<bool> SendNotification(string text)
    {
        using var response = await _httpClient.PostAsJsonAsync(_webhookUrl,
            new 
            {
                msgtype = "text", 
                text = new
                {
                    content = text
                }
            });
        return response.IsSuccessStatusCode;
    }
}

public sealed class NotificationSelector : INotificationSelector
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationSelector(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public INotification SelectNotification(string type)
    {
        if ("DingDing".EqualsIgnoreCase(type) 
            || "DingTalk".EqualsIgnoreCase(type)
            || "DingBot".EqualsIgnoreCase(type))
        {
            return _serviceProvider.GetServiceOrCreateInstance<DingTalkNotification>(); 
        }
        return _serviceProvider.GetServiceOrCreateInstance<GoogleChatNotification>();
    }
}
