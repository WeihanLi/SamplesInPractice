using System.Net;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeihanLi.Extensions;

namespace HttpClientTest
{
    public class NoAutoRedirectSample
    {
        private const string BaseUrl = "https://cn.iherb.com/blog/";
        public static async Task MainTest()
        {
            var articlePathString = @"
/8-natural-remedies-for-heartburn-and-acid-reflux/
/vitamin-d-food-sources/
/8-home-remedies-for-canker-sores/
/top-intermittent-fasting-tips/
/the-keto-diet-explained/
/8-natural-approaches-to-lowering-blood-pressure/
/8-benefits-of-rosehip-oil-for-the-skin/
/9-health-benefits-of-vitamin-b12/
/12-natural-remedies-for-dandruff/
/6-ways-to-manage-hashimotos-disease-naturally/
/turmeric-benefits-an-anti-inflammatory-spice/
";

            var articlePath = articlePathString.Split(new[]{ Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BaseUrl)
            };
            using var res = await httpClient.GetAsync(articlePath[0].TrimStart('/'));
            Console.WriteLine(res.RequestMessage.RequestUri.ToString());
            Console.WriteLine(res.StatusCode);
            Console.ReadLine();
            
            httpClient = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false
            })
            {
                BaseAddress = new Uri(BaseUrl)
            };
            using var res1 = await httpClient.GetAsync(articlePath[0].TrimStart('/'));
            Console.WriteLine(res1.RequestMessage.RequestUri.ToString());
            Console.WriteLine(res1.StatusCode);
            Console.ReadLine();

            var articleIds = await Task.WhenAll(articlePath.Select(async path =>
            {
                using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, path.TrimStart('/')));
                var statusCode = (int)response.StatusCode;
                if(statusCode != 302)
                {
                    return -1;
                }
                var newLocation = response.Headers.Location.ToString();
                int.TryParse(newLocation[(newLocation.LastIndexOf('/') + 1)..], out var articleId);
                return articleId;
            }));

            var json = new
            {
                articlelist = articleIds.Select(x=> new{ id=x })
            }.ToJson();
            Console.WriteLine(json);
        }
    }
}
