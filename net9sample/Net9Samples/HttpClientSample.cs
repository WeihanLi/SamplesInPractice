using Microsoft.Extensions.DependencyInjection;

namespace Net9Samples;

public class HttpClientSample
{
    public static async Task KeyedHttpClientSample()
    {
        var services = new ServiceCollection();
        services.ConfigureHttpClientDefaults(c =>
        {
            c.AddAsKeyed();
        });
        services.AddHttpClient("test1", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        })
            .AddAsKeyed()
            .RemoveAsKeyed()
            ;
        services.AddHttpClient("test2", client =>
        {
            client.BaseAddress = new Uri("http://localhost:6000");
        })
            .AddAsKeyed(ServiceLifetime.Singleton)
            ;
        
        await using var provider = services.BuildServiceProvider();
        {
            Console.WriteLine("----- scope 1 -----");
            await using var scope = provider.CreateAsyncScope();
            
            var client = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("test1");
            Console.WriteLine(client.GetHashCode());
            Console.WriteLine(client.BaseAddress);
            
            var client1 = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("test1");
            Console.WriteLine(client1.GetHashCode());
            Console.WriteLine(client1.BaseAddress);
        
            var client2 = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("test2");
            Console.WriteLine(client2.GetHashCode());
            Console.WriteLine(client2.BaseAddress);
        }
        {
            Console.WriteLine("----- scope 2 -----");
            await using var scope = provider.CreateAsyncScope();
            
            var client = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("test1");
            Console.WriteLine(client.GetHashCode());
            Console.WriteLine(client.BaseAddress);
        
            var client1 = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("test1");
            Console.WriteLine(client1.GetHashCode());
            Console.WriteLine(client1.BaseAddress);
        
            var client2 = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("test2");
            Console.WriteLine(client2.GetHashCode());
            Console.WriteLine(client2.BaseAddress);
        }
        {
            Console.WriteLine("----- scope 3 -----");
            await using var scope = provider.CreateAsyncScope();
            var httpClient = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("default");
            Console.WriteLine(httpClient.GetHashCode());
            Console.WriteLine(httpClient.BaseAddress);
            
            var httpClient2 = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("default");
            Console.WriteLine(httpClient2.GetHashCode());
        }
        {
            Console.WriteLine("----- scope 4 -----");
            await using var scope = provider.CreateAsyncScope();
            var httpClient = scope.ServiceProvider.GetRequiredKeyedService<HttpClient>("default");
            Console.WriteLine(httpClient.GetHashCode());
            Console.WriteLine(httpClient.BaseAddress);
        }
    }
}
