using System.Security.AccessControl;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Net8Sample;

public static class HttpClientSample
{
    public static async Task MainTest()
    {
        
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

    private static JsonTypeInfo<TValue> GetJsonTypeInfo<TValue>(JsonSerializerOptions? options)
    {
        var type = typeof(TValue);
        // Resolves JsonTypeInfo metadata using the appropriate JsonSerializerOptions configuration,
        // following the semantics of the JsonSerializer reflection methods.
        options ??= JsonSerializerOptions.Default;
        options.MakeReadOnly(populateMissingResolver: true);
        return (JsonTypeInfo<TValue>)options.GetTypeInfo(type);
    }
}

file sealed class Job
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public override string ToString() => $"JobId: {Id}, JobTitle: {Title}";
}
