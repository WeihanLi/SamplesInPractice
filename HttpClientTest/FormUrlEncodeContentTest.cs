using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WeihanLi.Common.Http;
using WeihanLi.Extensions;

namespace HttpClientTest
{
    public class FormUrlEncodeContentTest
    {
        private const string TestUrl = "https://cnblogs.com";

        public static async Task FormUrlEncodedContentLengthTest()
        {
            using (var httpClient = new HttpClient(new NoProxyHttpClientHandler()))
            {
                using (var response = await httpClient.PostAsync(TestUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    {"bigContent", new string('a', 65535)},
                })))
                {
                    Console.WriteLine($"response status code:{response.StatusCode}");
                }
            }
        }

        public static async Task ByteArrayContentLengthTest()
        {
            using (var httpClient = new HttpClient(new NoProxyHttpClientHandler()))
            {
                var postContent = $"bigContent={new string('a', 65535)}";
                using (var response = await httpClient.PostAsync(TestUrl, new ByteArrayContent(postContent.GetBytes())))
                {
                    Console.WriteLine($"response status code:{response.StatusCode}");
                }
            }
        }

        public static async Task StringContentLengthTest()
        {
            using (var httpClient = new HttpClient(new NoProxyHttpClientHandler()))
            {
                var postContent = $"bigContent={new string('a', 65535)}";
                using (var response = await httpClient.PostAsync(TestUrl, new StringContent(postContent)))
                {
                    Console.WriteLine($"response status code:{response.StatusCode}");
                }
            }
        }
    }
}
