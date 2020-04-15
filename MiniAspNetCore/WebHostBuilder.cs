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

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            return _server.StartAsync(_requestDelegate, cancellationToken);
        }
    }

    public interface IWebHostBuilder
    {
        IWebHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction);

        IWebHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureAction);

        IWebHostBuilder Init(Action<IConfiguration, IServiceProvider> initAction);

        IWebHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configureAction);

        IHost Build();
    }

    public class WebHostBuilder : IWebHostBuilder
    {
        private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();

        private IAsyncPipelineBuilder<HttpContext> _requestPipeline = PipelineBuilder.CreateAsync<HttpContext>(context =>
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        public IWebHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction)
        {
            configAction?.Invoke(_configurationBuilder);
            return this;
        }

        public IWebHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureAction)
        {
            if (null != configureAction)
            {
                var configuration = _configurationBuilder.Build();
                configureAction.Invoke(configuration, _serviceCollection);
            }

            return this;
        }

        public IWebHostBuilder Init(Action<IConfiguration, IServiceProvider> initAction)
        {
            if (null != initAction)
            {
                var configuration = _configurationBuilder.Build();
                var applicationServices = _serviceCollection.BuildServiceProvider();
                initAction.Invoke(configuration, applicationServices);
            }

            return this;
        }

        public IWebHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configureAction)
        {
            if (null != configureAction)
            {
                var configuration = _configurationBuilder.Build();
                configureAction?.Invoke(configuration, _requestPipeline);
            }
            return this;
        }

        public IHost Build()
        {
            var configuration = _configurationBuilder.Build();
            _serviceCollection.AddSingleton<IConfiguration>(configuration);
            return new WebHost(_serviceCollection.BuildServiceProvider(), _requestPipeline.Build());
        }

        public static WebHostBuilder CreateDefault()
        {
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureConfiguration(builder => builder.AddJsonFile("appsettings.json", true, true));
            webHostBuilder.ConfigureServices((configuration, services) => services.AddSingleton<IServer, HttpListenerServer>());

            return webHostBuilder;
        }
    }
}
