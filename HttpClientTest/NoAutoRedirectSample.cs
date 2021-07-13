using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeihanLi.Extensions;

namespace HttpClientTest
{
    public class NoAutoRedirectSample
    {
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
            // Console.WriteLine(articlePath.ToJson());
            
            var httpClient = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false
            })
            {
                BaseAddress = new Uri("https://www.iherb.com/blog/")
            };

            var articleIds = await Task.WhenAll(articlePath.Select(async path =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, path.TrimStart('/'));
                request.Headers.TryAddWithoutValidation("cookie", "ih-preference=store=0&country=US&language=en-US&currency=USD;iher-pref1=storeid=0&sccode=US&lan=en-US&scurcode=USD&pc=Nzg0MTU=&whr=1&wp=1&lchg=1&ifv=1;");
                using var response = await httpClient.SendAsync(request);
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
