using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using System.Collections.Concurrent;
using System.Net;

namespace OpenAISample;

public interface IOpenAIServiceFactory : IOpenAIService
{
    IOpenAIServiceWrapper GetService();

    void RateLimited(string name, TimeSpan expiresIn);
}

public sealed class OpenAIServiceFactory : IOpenAIServiceFactory
{
    private static readonly HashSet<string> ServicesRegistered = new();
    private readonly ConcurrentDictionary<string, OpenAIServiceWrapper> _services = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<OpenAiOptions> _openAIOptionsMonitor;
    private readonly IMemoryCache _memoryCache;

    public OpenAIServiceFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<OpenAiOptions> openAIOptionsMonitor,
        IMemoryCache memoryCache
    )
    {
        _httpClientFactory = httpClientFactory;
        _openAIOptionsMonitor = openAIOptionsMonitor;
        _memoryCache = memoryCache;
    }

    public IOpenAIServiceWrapper GetService(string name)
    {
        return _services.GetOrAdd(name, _ =>
        {
            var httpClient = _httpClientFactory.CreateClient(GetHttpClientName(_));
            var options = _openAIOptionsMonitor.Get(_);
            return new OpenAIServiceWrapper(_, new OpenAIService(options, httpClient))
            {
                IsRestricted = IsRestricted(_)
            };
        });
    }

    public IOpenAIServiceWrapper GetService()
    {
        if (ServicesRegistered.Count == 0) throw new InvalidOperationException("No service registered");

        if (_services.IsEmpty)
        {
            foreach (var name in ServicesRegistered)
            {
                GetService(name);
            }
        }

        return _services.Values.Where(s=> !s.IsRestricted).MinBy(_ => Random.Shared.Next(ServicesRegistered.Count * 10))
               ?? throw new InvalidOperationException("No valid service found");
    }

    public void RateLimited(string name, TimeSpan expiresIn) => _memoryCache.Set(GetRestrictedCacheKey(name), string.Empty, expiresIn);

    private bool IsRestricted(string name) => _memoryCache.TryGetValue(GetRestrictedCacheKey(name), out _);
    
    private static string GetRestrictedCacheKey(string name) => $"OpenAIService:RestrictedService:{name}";
    public static string GetHttpClientName(string? name = null) => $"OpenAIService_{name}";

    public static void RegisterService(string name)
    {
        ServicesRegistered.Add(name);
    }

    public void SetDefaultModelId(string modelId) => throw new NotSupportedException();

    public IModelService Models => GetService().Models;
    public ICompletionService Completions  => GetService().Completions;
    public IEmbeddingService Embeddings  => new EmbeddingServiceWrapper(this);
    public IFileService Files  => GetService().Files;
    public IFineTuneService FineTunes  => GetService().FineTunes;
    public IModerationService Moderation  => GetService().Moderation;
    public IImageService Image  => GetService().Image;
    public IEditService Edit  => GetService().Edit;
    public IChatCompletionService ChatCompletion  => new ChatCompletionServiceWrapper(this);
    public IAudioService Audio  => GetService().Audio;
}

public static class DependencyInjectionExtensions
{
    public static void RegisterOpenAIServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.Get<Dictionary<string, IConfigurationSection>>();
        if (options is null) return;
        foreach (var (name, config) in options)
        {
            services.AddOptions<OpenAiOptions>(name).Bind(config);
            services.AddHttpClient(OpenAIServiceFactory.GetHttpClientName(name))
                .AddHttpMessageHandler(_ => new CustomRateLimitedHttpHandler(name, _.GetRequiredService<IOpenAIServiceFactory>()));

            OpenAIServiceFactory.RegisterService(name);
        }
        services.TryAddSingleton<IOpenAIServiceFactory, OpenAIServiceFactory>();
        services.TryAddSingleton(sp => sp.GetRequiredService<IOpenAIServiceFactory>().GetService());
    }
}

public sealed class CustomRateLimitedHttpHandler : DelegatingHandler
{
    private readonly string _serviceName;
    private readonly IOpenAIServiceFactory _openAIServiceFactory;

    public CustomRateLimitedHttpHandler(string serviceName, IOpenAIServiceFactory openAIServiceFactory)
    {
        _serviceName = serviceName;
        _openAIServiceFactory = openAIServiceFactory;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var responseMessage = await base.SendAsync(request, cancellationToken);
        if (responseMessage.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _openAIServiceFactory.RateLimited(_serviceName, TimeSpan.FromMinutes(1));
        }
        return responseMessage;
    }
}
