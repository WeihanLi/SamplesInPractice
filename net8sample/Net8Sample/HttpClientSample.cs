using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        using var response = await httpClient.GetAsync("api/Jobs", (HttpCompletionOption)1);
        ArgumentNullException.ThrowIfNull(response.Content);
        var stream = response.Content.ReadFromJsonAsAsyncEnumerable<Job>();
        // var stream = httpClient.GetFromJsonAsAsyncEnumerable<Job>("api/Jobs", 
        //     new JsonSerializerOptions(JsonSerializerDefaults.Web)
        //     );
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
        await using var responseStream = await httpClient.GetStreamAsync("api/Jobs");
        var stream = JsonSerializer.DeserializeAsyncEnumerable<Job>(responseStream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await foreach (var job in stream)
        {
            Console.WriteLine(job);
            Console.WriteLine(DateTimeOffset.Now);
        }
    }
}

file sealed class Job
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public override string ToString() => $"JobId: {Id}, JobTitle: {Title}";
}
