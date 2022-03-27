using System.Net.Http.Json;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace HttpClientTest;

public class JsonExtensionSample
{
    public static async Task MainTest()
    {
        const string url = "https://reservation.weihanli.xyz/health";
        using var httpClient = new HttpClient();

        {
            using var response = await httpClient.PostAsJsonAsync(url, new Category()
            {
                Id = 1,
                Name = "Test"
            });
            response.EnsureSuccessStatusCode();
        }

        {
            using var response = await httpClient.PostAsJsonAsync(url, new Category()
            {
                Id = 1,
                Name = "Test"
            }, new JsonSerializerOptions());
            response.EnsureSuccessStatusCode();
        }

        {
            using var response = await httpClient.PutAsJsonAsync(url, new Category()
            {
                Id = 1,
                Name = "Test"
            });
            response.EnsureSuccessStatusCode();
        }

        {
            using var content = JsonContent.Create(new Category() { Id = 1, Name = "Test" });
            using var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var result = await content.ReadFromJsonAsync<Category>();
            ArgumentNullException.ThrowIfNull(result);
            Console.WriteLine($"{result.Id}: {result.Name}");
        }

        {
            var result = await httpClient.GetFromJsonAsync<Result>(url);
            ArgumentNullException.ThrowIfNull(result);
            Console.WriteLine($"{result.Status}: {result.Msg}");
        }

        {
            var result = await httpClient.GetFromJsonAsync<Result>(url, new JsonSerializerOptions());
            ArgumentNullException.ThrowIfNull(result);
            Console.WriteLine($"{result.Status}: {result.Msg}");
        }
    }
}
