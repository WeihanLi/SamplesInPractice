using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using WeihanLi.Common.Helpers;

namespace HttpClientTest;

public static class AsyncEnumerableSample
{
    public static async Task MainTest()
    {
        var server = await StartupServer();
        
        var url = "http://localhost:5000/api/values";
        var url1 = "http://localhost:5000/api/values/async";
        {
            Console.WriteLine($"Send request at {DateTimeOffset.Now}");
            using var httpClient = new HttpClient();
            await foreach (var (key, value) in httpClient.GetFromJsonAsAsyncEnumerable<KeyValuePair<string, string>>(url))
            {
                Console.WriteLine($"Received response at {DateTimeOffset.Now} : {key} {value}");
            }
        }
        Console.WriteLine(new string('-', 50));
        {
            // https://github.com/dotnet/runtime/issues/102113
            Console.WriteLine($"Send request at {DateTimeOffset.Now}");
            using var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });
            await foreach (var (key, value) in httpClient.GetFromJsonAsAsyncEnumerable<KeyValuePair<string, string>>(url))
            {
                Console.WriteLine($"Received response at {DateTimeOffset.Now} : {key} {value}");
            }
        }

        Console.WriteLine(new string('-', 50));
        {
            Console.WriteLine($"Send request at {DateTimeOffset.Now}");
            using var httpClient = new HttpClient();
            await foreach (var (key, value) in httpClient.GetFromJsonAsAsyncEnumerable<KeyValuePair<string, string>>(url1))
            {
                Console.WriteLine($"Received response at {DateTimeOffset.Now} : {key} {value}");
            }
        }
        Console.WriteLine(new string('-', 50));
        {
            // https://github.com/dotnet/runtime/issues/102113
            Console.WriteLine($"Send request at {DateTimeOffset.Now}");
            using var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });
            await foreach (var (key, value) in httpClient.GetFromJsonAsAsyncEnumerable<KeyValuePair<string, string>>(url1))
            {
                Console.WriteLine($"Received response at {DateTimeOffset.Now} : {key} {value}");
            }
        }

        await server.StopAsync();
    }
    
    private static async Task<WebApplication> StartupServer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddControllers();
        builder.Services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
        });
        var app = builder.Build();
        app.Urls.Clear();
        app.Urls.Add("http://localhost:5000");
        app.UseResponseCompression();
        app.Map("/m/api/values", TestHelper.GetValues);
        app.Map("/m/api/values1", TestHelper.GetValuesAsync);
        app.MapControllers();
        await app.StartAsync();
        Console.WriteLine("Server started");
        return app;
    }
}

file static class TestHelper
{
    public static async IAsyncEnumerable<KeyValuePair<string, string>> GetValues()
    {
        for (var i = 0; i < 3; i++)
        {
            yield return new KeyValuePair<string, string>("test", "test");
            await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
            Thread.Sleep(2000);
        }
    }
        
    public static async IAsyncEnumerable<KeyValuePair<string, string>> GetValuesAsync()
    {
        for (var i = 0; i < 3; i++)
        {
            yield return new KeyValuePair<string, string>("test", "async test");
            await Task.Delay(2000);
        }
    }
}

[Route("api/values")]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public IAsyncEnumerable<KeyValuePair<string, string>> GetValues() => TestHelper.GetValues();
    
    [HttpGet("async")]
    public IAsyncEnumerable<KeyValuePair<string, string>> GetValuesAsync() => TestHelper.GetValuesAsync();
}
