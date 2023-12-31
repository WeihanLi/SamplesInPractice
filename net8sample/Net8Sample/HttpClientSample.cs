using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using WeihanLi.Common.Helpers;

namespace Net8Sample;

public static class HttpClientSample
{
    public static async Task MainTest()
    {
        await HttpClientGetFromJsonAsAsyncEnumerableSample();
        await HttpClientGetStreamDeserializeAsyncEnumerableSample();
        await HttpClientHttpResilienceSample();
        await ConfigureHttpClientDefaultsSample();
    }

    public static async Task HttpClientGetFromJsonAsAsyncEnumerableSample()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5297");
        // using var response = await httpClient.GetAsync("api/Jobs", HttpCompletionOption.ResponseHeadersRead);
        // ArgumentNullException.ThrowIfNull(response.Content);
        // var stream = response.Content.ReadFromJsonAsAsyncEnumerable<Job>();

        // var jsonTypeInfo = GetJsonTypeInfo<Job>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        // var stream = httpClient.GetFromJsonAsAsyncEnumerable("api/Jobs", jsonTypeInfo);
                
        var stream = httpClient.GetFromJsonAsAsyncEnumerable<Job>("api/Jobs");
                
        await foreach (var job in stream)
        {
            Console.WriteLine(job);
            Console.WriteLine(DateTimeOffset.Now);
        }
    }
    
    public static async Task HttpClientGetStreamDeserializeAsyncEnumerableSample()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5297");
        await using var responseStream = await httpClient.GetStreamAsync("/api/jobs");
        var stream = JsonSerializer.DeserializeAsyncEnumerable<Job>(responseStream, 
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await foreach (var job in stream)
        {
            Console.WriteLine(job);
            Console.WriteLine(DateTimeOffset.Now);
        }
    }

    public static async Task HttpClientHttpResilienceSample()
    {
        var services = new ServiceCollection();
        services.AddTransient<MyHttpDelegatingHandler>();
        services.ConfigureHttpClientDefaults(x =>
        {
            x.AddStandardResilienceHandler();
            x.AddHttpMessageHandler<MyHttpDelegatingHandler>();
        });
        services.AddHttpClient("reservation-client", client =>
            {
                client.BaseAddress = new Uri("https://reservtaion1.weihanli1.xyz/");
            });
        services.AddHttpClient("spark-client", client =>
        {
            client.BaseAddress = new Uri("https://spark1.weihanli1.xyz/");
        });
        
        
        await using var serviceProvider = services.BuildServiceProvider();
        var reservationClient = serviceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient("reservation-client");
        var sparkClient = serviceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient("spark-client");
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        while (await timer.WaitForNextTickAsync())
        {
            Console.WriteLine("Tick triggered: ");
            try
            {
                var reservationHealthResponse = await reservationClient.GetStringAsync("/health");
                Console.WriteLine(reservationHealthResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine("ReservationException");
                Console.WriteLine(e);
            }

            try
            {
                var sparkHealthResponse = await sparkClient.GetStringAsync("/health");
                Console.WriteLine(sparkHealthResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine("SparkException");
                Console.WriteLine(e);
            }
        }
        
        Console.ReadLine();
    }

    // https://github.com/dotnet/runtime/pull/87953
    // https://github.com/dotnet/runtime/issues/87914
    // https://github.com/dotnet/runtime/blob/v8.0.0/src/libraries/Microsoft.Extensions.Http/src/DependencyInjection/DefaultHttpClientBuilderServiceCollection.cs
    public static async Task ConfigureHttpClientDefaultsSample()
    {
        var services = new ServiceCollection();
        services.AddTransient<MyHttpDelegatingHandler>();
        services.AddTransient<MyHttpDelegatingHandler2>();
        services.AddTransient<MyHttpDelegatingHandler3>();
        services.ConfigureHttpClientDefaults(x =>
        {
            x.AddHttpMessageHandler<MyHttpDelegatingHandler>();
        });
        services.AddHttpClient("reservation-client", client =>
        {
            client.BaseAddress = new Uri("https://reservation.weihanli.xyz");
        }).AddHttpMessageHandler<MyHttpDelegatingHandler2>();
        services.AddHttpClient("spark-client", client =>
        {
            client.BaseAddress = new Uri("https://spark.weihanli.xyz");
        });
        services.ConfigureHttpClientDefaults(x =>
        {
            x.AddHttpMessageHandler<MyHttpDelegatingHandler3>();
        });
        
        services.DumpHttpClientFactoryOptionsConfigureService();
        
        await using var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        await InvokeHelper.TryInvokeAsync(async () =>
        {
            var responseText = await httpClientFactory.CreateClient("reservation-client")
                .GetStringAsync("/health");
            Console.WriteLine(responseText);
        });
        Console.WriteLine();
        //
        await InvokeHelper.TryInvokeAsync(() => httpClientFactory.CreateClient("spark-client")
            .GetStringAsync("/"));
    }
    
    public static async Task ConfigureHttpClientDefaultsSample2()
    {
        var services = new ServiceCollection();
        services.AddTransient<MyHttpDelegatingHandler>();
        services.AddTransient<MyHttpDelegatingHandler2>();
        services.AddTransient<MyHttpDelegatingHandler3>();
        services.AddHttpClient("spark-client", client =>
        {
            client.BaseAddress = new Uri("https://spark.weihanli.xyz");
        }).AddHttpMessageHandler<MyHttpDelegatingHandler2>();
        services.ConfigureHttpClientDefaults(x =>
        {
            x.AddHttpMessageHandler<MyHttpDelegatingHandler>();
            x.AddHttpMessageHandler<MyHttpDelegatingHandler3>();
        });
        services.AddHttpClient("reservation-client", client =>
        {
            client.BaseAddress = new Uri("https://reservation.weihanli.xyz");
        }).AddHttpMessageHandler<MyHttpDelegatingHandler2>();
        
        services.DumpHttpClientFactoryOptionsConfigureService();
        
        await using var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        await InvokeHelper.TryInvokeAsync(async () =>
        {
            var responseText = await httpClientFactory.CreateClient("reservation-client")
                .GetStringAsync("/health");
            Console.WriteLine(responseText);
        });
        Console.WriteLine();
        //
        await InvokeHelper.TryInvokeAsync(() => httpClientFactory.CreateClient("spark-client")
            .GetStringAsync("/"));
    }

    //
    public static async Task HttpClientLoggerSample()
    {
    }

    private static void DumpHttpClientFactoryOptionsConfigureService(this IServiceCollection services)
    {
        Console.WriteLine();
        foreach (var configure in services
                     .Where(x => x.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
                     .Select(x=> x.ImplementationInstance)
                     .OfType<ConfigureNamedOptions<HttpClientFactoryOptions>>())
        {
            Console.WriteLine($"Configure HttpClientName: {configure.Name}");
        }
        Console.WriteLine();
    }
}

file sealed class Job
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public override string ToString() => $"JobId: {Id}, JobTitle: {Title}";
}

file sealed class MyHttpDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending request in {nameof(MyHttpDelegatingHandler)}...");
        return base.SendAsync(request, cancellationToken);
    }
}

file sealed class MyHttpDelegatingHandler2 : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending request in {nameof(MyHttpDelegatingHandler2)}...");
        return base.SendAsync(request, cancellationToken);
    }
}
file sealed class MyHttpDelegatingHandler3 : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending request in {nameof(MyHttpDelegatingHandler3)}...");
        return base.SendAsync(request, cancellationToken);
    }
}
file sealed class MyHttpClientLogger : IHttpClientLogger
{
    public object? LogRequestStart(HttpRequestMessage request)
    {
        throw new NotImplementedException();
    }

    public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
    {
        throw new NotImplementedException();
    }

    public void LogRequestFailed(object? context, HttpRequestMessage request, HttpResponseMessage? response, Exception exception,
        TimeSpan elapsed)
    {
        throw new NotImplementedException();
    }
}
