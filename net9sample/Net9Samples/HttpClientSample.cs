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
            //.AddAsKeyed()
            ;
        services.AddHttpClient("test2", client =>
        {
            client.BaseAddress = new Uri("http://localhost:6000");
        })
            .AddAsKeyed()
            ;
        await using var provider = services.BuildServiceProvider();
        
        var client1 = provider.GetRequiredKeyedService<HttpClient>("test1");
        Console.WriteLine(client1.BaseAddress);
        
        var client2 = provider.GetRequiredKeyedService<HttpClient>("test2");
        Console.WriteLine(client2.BaseAddress);
    }
}
