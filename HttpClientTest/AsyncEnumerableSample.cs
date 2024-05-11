using System.Net;
using System.Net.Http.Json;
using WeihanLi.Common.Helpers;

namespace HttpClientTest;

public static class AsyncEnumerableSample
{
    public static async Task MainTest()
    {
        var url = "http://localhost:5000/api/Values/en-US/single/IDS_WEIHAN_TEST/allCountries?includeGlobal=true";
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
            Console.WriteLine($"Send request at {DateTimeOffset.Now}");
            var postUrl = "http://localhost:5000/api/Values/en-US/entry/allCountries";
            using var httpClient = new HttpClient();
            using var response = await httpClient.PostAsync(postUrl,
                JsonContent.Create(new { cmsKeyNames = new[] { "IDS_CMS_SDK_TEST", "IDS_WEIHAN_TEST" } }));
            await foreach (var entry in response.Content.ReadFromJsonAsAsyncEnumerable<KeyEntry>())
            {
                Console.WriteLine($"Received response at {DateTimeOffset.Now} : {entry}");
            }
        }
        
        Console.WriteLine(new string('-', 50));
        {
            Console.WriteLine($"Send request at {DateTimeOffset.Now}");
            var postUrl = "http://localhost:5000/api/Values/en-US/entry/allCountries";
            using var httpClient = new HttpClient();
            using var requestContent = JsonContent.Create(new { cmsKeyNames = new[] { "IDS_CMS_SDK_TEST", "IDS_WEIHAN_TEST" } }); 
            using var request = new HttpRequestMessage(HttpMethod.Post, postUrl);
            request.Content = requestContent;
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            await foreach (var entry in response.Content.ReadFromJsonAsAsyncEnumerable<KeyEntry>())
            {
                Console.WriteLine($"Received response at {DateTimeOffset.Now} : {entry}");
            }
        }

        // Console.WriteLine(new string('-', 50));
        //
        // {
        //     Console.WriteLine($"Send request at {DateTimeOffset.Now}");
        //     await foreach (var (key, value) in HttpHelper.HttpClient.GetFromJsonAsAsyncEnumerable<KeyValuePair<string, string>>(url))
        //     {
        //         Console.WriteLine($"Received response at {DateTimeOffset.Now} : {key} {value}");
        //     }
        // }
        
        // Console.WriteLine(new string('-', 50));
        //
        // {
        //     // https://github.com/dotnet/runtime/issues/102113
        //     Console.WriteLine($"Send request at {DateTimeOffset.Now}");
        //     using var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });
        //     await foreach (var (key, value) in httpClient.GetFromJsonAsAsyncEnumerable<KeyValuePair<string, string>>(url))
        //     {
        //         Console.WriteLine($"Received response at {DateTimeOffset.Now} : {key} {value}");
        //     }
        // }
    }
}

file sealed record KeyEntry(string Key, string Value, string Country);
