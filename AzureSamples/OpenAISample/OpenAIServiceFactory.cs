using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Managers;
using System.Collections.Concurrent;

namespace OpenAISample;

public interface IOpenAIServiceFactory
{
    IOpenAIServiceWrapper GetService(string name);

    IOpenAIServiceWrapper GetService();
}

public sealed class OpenAIServiceFactory : IOpenAIServiceFactory
{
    private static readonly HashSet<string> ServicesRegistered = new();
    private readonly ConcurrentDictionary<string, OpenAIServiceWrapper> _services = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<OpenAiOptions> _openAIOptionsMonitor;

    public OpenAIServiceFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<OpenAiOptions> openAIOptionsMonitor
    )
    {
        _httpClientFactory = httpClientFactory;
        _openAIOptionsMonitor = openAIOptionsMonitor;
    }

    public IOpenAIServiceWrapper GetService(string name)
    {
        return _services.GetOrAdd(name, _ =>
        {
            var httpClient = _httpClientFactory.CreateClient(GetHttpClientName(_));
            var options = _openAIOptionsMonitor.Get(_);
            return new OpenAIServiceWrapper(_, new OpenAIService(options, httpClient));
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

        return _services.Values.OrderBy(_ => Random.Shared.Next(ServicesRegistered.Count * 10)).First();
    }

    public static string GetHttpClientName(string? name = null) => $"OpenAIService_{name}";

    public static void RegisterService(string name)
    {
        ServicesRegistered.Add(name);
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
            services.AddHttpClient(OpenAIServiceFactory.GetHttpClientName(name));

            OpenAIServiceFactory.RegisterService(name);
        }
        services.TryAddSingleton<IOpenAIServiceFactory, OpenAIServiceFactory>();
        services.TryAddSingleton(sp => sp.GetRequiredService<IOpenAIServiceFactory>().GetService());
    }
}
