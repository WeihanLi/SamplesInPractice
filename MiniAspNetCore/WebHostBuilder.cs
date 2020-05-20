using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Helpers;

namespace MiniAspNetCore
{
    public interface IHost
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }

    public class WebHost : IHost
    {
        private readonly Func<HttpContext, Task> _requestDelegate;
        private readonly IServer _server;

        public WebHost(IServiceProvider serviceProvider, Func<HttpContext, Task> requestDelegate)
        {
            _requestDelegate = requestDelegate;
            _server = serviceProvider.GetRequiredService<IServer>();
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(() => _server.StopAsync().RunSynchronously());
            await _server.StartAsync(_requestDelegate, cancellationToken).ConfigureAwait(false);
        }
    }

    public interface IHostBuilder
    {
        IHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction);

        IHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureAction);

        IHostBuilder Initialize(Action<IConfiguration, IServiceProvider> initAction);

        IHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configureAction);

        IHost Build();
    }

    public class WebHostBuilder : IHostBuilder
    {
        private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();

        private Action<IConfiguration, IServiceProvider> _initAction = null;

        private readonly IAsyncPipelineBuilder<HttpContext> _requestPipeline = PipelineBuilder.CreateAsync<HttpContext>(context =>
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        public IHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction)
        {
            configAction?.Invoke(_configurationBuilder);
            return this;
        }

        public IHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureAction)
        {
            if (null != configureAction)
            {
                var configuration = _configurationBuilder.Build();
                configureAction.Invoke(configuration, _serviceCollection);
            }

            return this;
        }

        public IHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configureAction)
        {
            if (null != configureAction)
            {
                var configuration = _configurationBuilder.Build();
                configureAction.Invoke(configuration, _requestPipeline);
            }
            return this;
        }

        public IHostBuilder Initialize(Action<IConfiguration, IServiceProvider> initAction)
        {
            if (null != initAction)
            {
                _initAction = initAction;
            }

            return this;
        }

        public IHost Build()
        {
            var configuration = _configurationBuilder.Build();
            _serviceCollection.AddSingleton<IConfiguration>(configuration);
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            _initAction?.Invoke(configuration, serviceProvider);

            return new WebHost(serviceProvider, _requestPipeline.Build());
        }

        public static WebHostBuilder CreateDefault(string[] args)
        {
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder
                .ConfigureConfiguration(builder => builder.AddJsonFile("appsettings.json", true, true))
                .UseHttpListenerServer()
                ;

            return webHostBuilder;
        }
    }

    public static class WebHostBuilderExtensions
    {
        public static IHostBuilder UseHttpListenerServer(this IHostBuilder builder)
        {
            return builder.ConfigureServices((configuration, services) =>
                {
                    services.AddSingleton<IServer, HttpListenerServer>();
                });
        }
    }
}
