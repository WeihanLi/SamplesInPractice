using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Extensions;

namespace MiniAspNetCore
{
    public class HttpListenerServer : IServer
    {
        private readonly HttpListener _listener;
        private readonly IServiceProvider _serviceProvider;

        public HttpListenerServer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _listener = new HttpListener();
            var urls = configuration.GetAppSetting("ASPNETCORE_URLS")?.Split(';');
            if (urls != null && urls.Length > 0)
            {
                foreach (var url in urls.Select(u => u?.Trim()).Where(u => u.IsNullOrEmpty()).Distinct())
                {
                    // Prefixes must end in a forward slash ("/") https://stackoverflow.com/questions/26157475/use-of-httplistener
                    _listener.Prefixes.Add(url.EndsWith("/") ? url : $"{url}/");
                }
            }
            else
            {
                _listener.Prefixes.Add("http://localhost:5100/");
            }

            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(Func<HttpContext, Task> requestHandler, CancellationToken cancellationToken = default)
        {
            _listener.Start();
            if (_listener.IsListening)
            {
                Console.WriteLine("the server is listening on ");
                Console.WriteLine(_listener.Prefixes.StringJoin(","));
            }
            while (true)
            {
                var listenerContext = await _listener.GetContextAsync();

                var featureCollection = new FeatureCollection();
                featureCollection.Set(listenerContext.GetRequestFeature());
                featureCollection.Set(listenerContext.GetResponseFeature());

                using (var scope = _serviceProvider.CreateScope())
                {
                    var httpContext = new HttpContext(featureCollection)
                    {
                        RequestServices = scope.ServiceProvider,
                    };

                    await requestHandler(httpContext);
                }
                listenerContext.Response.Close();
            }
        }

        public Task StopAsync()
        {
            _listener.Stop();
            return Task.CompletedTask;
        }
    }
}
