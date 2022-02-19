using System.Net.Http.Json;

namespace Net7Sample;
public class HttpClientJsonSample
{
    public static async Task MainTest()
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.PatchAsJsonAsync("https://reservation.weihanli.xyz/health", new 
        { 
            Name = "test",
            Age = 10
        });
        response.EnsureSuccessStatusCode();
        Console.WriteLine(response.StatusCode.ToString());
    }
}
