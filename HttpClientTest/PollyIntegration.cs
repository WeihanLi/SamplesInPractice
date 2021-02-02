using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace HttpClientTest
{
    public class PollyIntegration
    {
        public static async Task MainTest()
        {
            var services = new ServiceCollection();
            services.AddHttpClient("test")
                .AddTransientHttpErrorPolicy(builder => builder.RetryAsync(5))
                ;

            await using var provider = services.BuildServiceProvider();
            await provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("test")
                .GetAsync("");
        }
    }
}
