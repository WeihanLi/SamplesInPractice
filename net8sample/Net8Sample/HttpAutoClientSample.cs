using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.AutoClient;
using Microsoft.Extensions.Logging;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace Net8Sample;

public static class HttpAutoClientSample
{
    public static async Task MainTest()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddHttpClient(nameof(INoticeClient), client =>
            {
                client.BaseAddress = new Uri("https://reservation.weihanli.xyz");
            });
        // services.AddNoticeClient();
        services.AddSingleton<NoticeService>();

        await using var serviceProvider = services.BuildServiceProvider();
        var noticeService = serviceProvider.GetRequiredService<NoticeService>();
        var result = await noticeService.GetList(1, 5);
        Console.WriteLine(result.ToJson());
        Console.WriteLine();
        var path = result.Data[0].NoticeCustomPath;
        if (!string.IsNullOrEmpty(path))
        {
            var noticeClient = serviceProvider.GetRequiredService<INoticeClient>();
            using var response = await noticeClient.GetDetailResponse("1234");
            Console.WriteLine(response.StatusCode);
            Console.WriteLine();

            var responseText = await noticeClient.GetDetailText(path);
            Console.WriteLine(responseText);
            Console.WriteLine();

            using var postResponse = await noticeClient.Post(new Notice()
            {
                NoticeTitle = "yest",
                NoticePublishTime = DateTime.UtcNow,
                NoticeExternalLink = "https://google.com"
            });
            Console.WriteLine(postResponse.StatusCode);
        }
    }
}

// [AutoClient(nameof(INoticeClient))]
public interface INoticeClient
{
    [Get("/api/notice")]
    [StaticHeader("X-RequestedBy", "AutoClient")]
    Task<PagedListResult<Notice>> GetList([Query]int pageNumber, [Query]int? pageSize = 10, CancellationToken cancellationToken = default);
    
    [Get("/api/notice/{path}")]
    Task<HttpResponseMessage> GetDetailResponse(string path, CancellationToken cancellationToken = default);
    
    [Get("/api/notice/{path}")]
    Task<string> GetDetailText(string path, CancellationToken cancellationToken = default);

    [Post("/health")]
    Task<HttpResponseMessage> Post([Body]Notice notice, CancellationToken cancellationToken = default);
}

public sealed class Notice
{
    public required string NoticeTitle { get; set; }
    public required DateTime NoticePublishTime { get; set; }
    public string? NoticeCustomPath { get; set; }
    public string? NoticeExternalLink { get; set; }
}

file class NoticeService(INoticeClient noticeClient)
{
    public Task<PagedListResult<Notice>> GetList(int pageNum, int pageSize,
        CancellationToken cancellationToken = default)
        => noticeClient.GetList(pageNum, pageSize, cancellationToken);
}
