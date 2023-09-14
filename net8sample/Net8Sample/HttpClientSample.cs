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

    public static async Task HttpClientJsonAsyncEnumerableSample()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5297");
        var stream = httpClient.GetFromJsonAsAsyncEnumerable<Job>("api/Jobs");
        await foreach (var job in stream)
        {
            Console.WriteLine(job);
            Console.WriteLine(DateTimeOffset.Now);
        }
    }
}

file sealed class Job
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    public override string ToString() => $"JobId: {Id}, JobTitle: {Title}";
}
