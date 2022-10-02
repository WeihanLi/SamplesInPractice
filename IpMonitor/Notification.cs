using System.Net.Http.Json;
using WeihanLi.Common;

namespace IpMonitor;

public interface INotification
{
    Task<bool> SendNotification(string text);
}

public sealed class GoogleChatNotification: INotification
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public GoogleChatNotification(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = Guard.NotNullOrEmpty(configuration.GetAppSetting("GChatWebHookUrl"));
    }
    
    public async Task<bool> SendNotification(string text)
    {
        // https://developers.google.com/chat/api/guides/message-formats/basic
        using var response = await _httpClient.PostAsJsonAsync(_webhookUrl, new { text });
        return response.IsSuccessStatusCode;
    }
}
