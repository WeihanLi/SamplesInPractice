using AngleSharp.Io;
using System.Net;

namespace HttpClientTest;

public class AuthHandlerSample
{
    public static async Task MainTest()
    {
        var services = new ServiceCollection();
        services.AddTransient<QueryAuthHandler>();
        services.AddTransient<DynamicTokenHandler>();
        services.AddHttpClient("auth", client =>
            {
                client.BaseAddress = new Uri("https://reservation.weihanli.xyz/");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "HttpClientFactory-Sample");
                client.DefaultRequestHeaders.TryAddWithoutValidation("ApiKey", "apiKey token-here");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                Proxy = new WebProxy("http://localhost:8888")
            })
            .AddHttpMessageHandler<QueryAuthHandler>()
            .AddHttpMessageHandler<DynamicTokenHandler>();
        await using var provider = services.BuildServiceProvider();
        var httpClient = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient("auth");
        var responseText = await httpClient.GetStringAsync("health");
        Console.WriteLine(responseText);
    }
}

file sealed class QueryAuthHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var hasQuestionMark = !string.IsNullOrEmpty(request.RequestUri.Query);
        request.RequestUri = new Uri($"{request.RequestUri.OriginalString}{(hasQuestionMark ? "&" : "?")}apiKey=test");
        return base.SendAsync(request, cancellationToken);
    }
}

file sealed class DynamicTokenHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = GetToken();
        request.Headers.TryAddWithoutValidation(HeaderNames.Authorization, $"Bearer {token}");
        return base.SendAsync(request, cancellationToken);
    }
    private string GetToken()
    {
        // In a real application, this method would retrieve a token from a secure source.
        // memoryCache.GetOrCreateAsync
        return $"dynamic-token-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
