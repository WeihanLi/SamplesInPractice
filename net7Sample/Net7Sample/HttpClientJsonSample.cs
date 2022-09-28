using System.Net.Http.Json;
using WeihanLi.Common.Models;

namespace Net7Sample;
public class HttpClientJsonSample
{
    public static async Task MainTest()
    {
        using var httpClient = new HttpClient();
        var url = "https://reservation.weihanli.xyz/health";

        using var response = await httpClient.PatchAsJsonAsync(url, new 
        { 
            Name = "test",
            Age = 10
        });
        response.EnsureSuccessStatusCode();
        Console.WriteLine(response.StatusCode.ToString());

        var result = await httpClient.DeleteFromJsonAsync<Result>(url);
        System.Console.WriteLine(result);
    }
}
