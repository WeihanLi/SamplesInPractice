using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WeihanLi.Common;

namespace IpMonitor;

public interface INotification
{
    string NotificationType { get; }
    Task<bool> SendNotification(string text);
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
        using var response = await _httpClient.PostAsJsonAsync(_webhookUrl,
            new GoogleChatRequestMsgModel { Text = text },
            NotificationRequestSerializationContext.Default.GoogleChatRequestMsgModel
            );
        return response.IsSuccessStatusCode;
    }
}

public sealed class DingBotNotification : INotification
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public DingBotNotification(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = Guard.NotNullOrEmpty(configuration.GetAppSetting("WebhookUrl"));
    }

    public string NotificationType => "DingBot";

    public async Task<bool> SendNotification(string text)
    {
        using var response = await _httpClient.PostAsJsonAsync(_webhookUrl,
            new DingBotTextRequestModel
            {
                Text = new()
                {
                    Content = text
                }
            }, NotificationRequestSerializationContext.Default.DingBotTextRequestModel);
        return response.IsSuccessStatusCode;
    }
}

public sealed class GoogleChatRequestMsgModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public sealed class DingBotTextRequestModel
{
    [JsonPropertyName("msgtype")]
    public string MsgType => "text";

    [JsonPropertyName("text")]
    public required TextModel Text { get; set; }

    public sealed class TextModel
    {
        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }
}

[JsonSerializable(typeof(GoogleChatRequestMsgModel))]
[JsonSerializable(typeof(DingBotTextRequestModel))]
public partial class NotificationRequestSerializationContext : JsonSerializerContext 
{
}
