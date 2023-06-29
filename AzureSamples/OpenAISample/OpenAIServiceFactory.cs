using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net;

namespace OpenAISample;

public interface IOpenAIServiceFactory : IOpenAIService
{
    int ServiceCount { get; }
    
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

    private static readonly ImmutableArray<TimeSpan> RestrictedDelay = new[]
    {
        TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), 
        TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(40),
    }.ToImmutableArray();
    
    public OpenAIServiceFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<OpenAiOptions> openAIOptionsMonitor,
        IMemoryCache memoryCache
    )
    {
        _httpClientFactory = httpClientFactory;
        _openAIOptionsMonitor = openAIOptionsMonitor;
        _memoryCache = memoryCache;
        
        EnsureInitialized();
    }

    public int ServiceCount => _services.Count;

    public IOpenAIServiceWrapper GetService()
    {
        if (ServicesRegistered.Count == 0) throw new InvalidOperationException("No service registered");

        var service = GetServiceInternal();
        if (service != null) return service;

        var retry = 0;
        while (service is null && retry < RestrictedDelay.Length)
        {
            Thread.Sleep(RestrictedDelay[retry]);
            retry++;
            service = GetServiceInternal();
        }
        return service ?? throw new InvalidOperationException("No valid service found");

        IOpenAIServiceWrapper? GetServiceInternal()
        {
            var availableServices = _services.Values.Where(s => !IsRestricted(s.Name)).ToArray();
            return availableServices.Length switch
            {
                0 => null,
                1 => availableServices[0],
                _ => availableServices[Random.Shared.Next(availableServices.Length)]
            };
        }
    }

    public void RateLimited(string name, TimeSpan expiresIn) => _memoryCache.Set(GetRestrictedCacheKey(name), string.Empty, expiresIn);

    private bool IsRestricted(string name) => _memoryCache.TryGetValue(GetRestrictedCacheKey(name), out _);
    
    private static string GetRestrictedCacheKey(string name) => $"OpenAIService:RestrictedService:{name}";
    public static string GetHttpClientName(string? name = null) => $"OpenAIService_{name}";

    public static void RegisterService(string name) => ServicesRegistered.Add(name);

    #region OpenAI Service
    
    public void SetDefaultModelId(string modelId)
    {
        foreach (var serviceWrapper in _services.Values)
        {
            serviceWrapper.Service.SetDefaultModelId(modelId);
        }
    }

    public IModelService Models => GetService().Service.Models;
    public ICompletionService Completions  => GetService().Service.Completions;
    public IEmbeddingService Embeddings  => new EmbeddingServiceWrapper(this);
    public IFileService Files  => GetService().Service.Files;
    public IFineTuneService FineTunes  => GetService().Service.FineTunes;
    public IModerationService Moderation  => GetService().Service.Moderation;
    public IImageService Image  => GetService().Service.Image;
    public IEditService Edit  => GetService().Service.Edit;
    public IChatCompletionService ChatCompletion  => new ChatCompletionServiceWrapper(this);
    public IAudioService Audio  => GetService().Service.Audio;
    
    #endregion OpenAI Service
    
    private void EnsureInitialized()
    {
        if (_services.IsEmpty)
        {
            foreach (var name in ServicesRegistered)
            {
                _services.GetOrAdd(name, n =>
                {
                    var httpClient = _httpClientFactory.CreateClient(GetHttpClientName(n));
                    var options = _openAIOptionsMonitor.Get(n);
                    return new OpenAIServiceWrapper(n, new OpenAIService(options, httpClient));
                });
            }
        }
    }
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
                .AddHttpMessageHandler(sp => new CustomRateLimitedHttpHandler(name, sp.GetRequiredService<IOpenAIServiceFactory>()));

            OpenAIServiceFactory.RegisterService(name);
        }
        services.TryAddSingleton<IOpenAIServiceFactory, OpenAIServiceFactory>();
        services.TryAddSingleton<IOpenAIService>(sp => sp.GetRequiredService<IOpenAIServiceFactory>());
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
        // https://help.openai.com/en/articles/6891839-api-error-code-guidance
        // https://help.openai.com/en/articles/7416438-rate-limits
        TimeSpan? expiresIn = responseMessage.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => TimeSpan.FromMinutes(1),
            HttpStatusCode.Unauthorized => TimeSpan.MaxValue,
            HttpStatusCode.NotFound => TimeSpan.MaxValue,
            _ => null
        };
        if (expiresIn.HasValue)
        {
            _openAIServiceFactory.RateLimited(_serviceName, expiresIn.Value);   
        }
        return responseMessage;
    }
}
