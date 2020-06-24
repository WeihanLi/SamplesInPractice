using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeihanLi.Common.Http;

namespace NugetSample
{
    public class RawApiSample
    {
        public const string NugetServiceIndex = "https://api.nuget.org/v3/index.json";

        public static async Task Test()
        {
            using (var httpClient = new HttpClient(new NoProxyHttpClientHandler()))
            {
                // loadServiceIndex
                var serviceIndexResponse = await httpClient.GetStringAsync(NugetServiceIndex);
                var serviceIndexObject = JObject.Parse(serviceIndexResponse);

                var keyword = "weihanli";

                // https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource
                //var queryEndpoint = serviceIndexObject["resources"]
                //    .First(x => x["@type"].Value<string>() == "SearchQueryService")["@id"]
                //    .Value<string>();
                //var queryUrl = $"{queryEndpoint}?q={keyword}&skip=0&take=5&prerelease=false&semVerLevel=2.0.0";
                //var queryResponse = await httpClient.GetStringAsync(queryUrl);
                //Console.WriteLine($"formatted queryResponse:");
                //Console.WriteLine($"{JObject.Parse(queryResponse).ToString(Formatting.Indented)}");

                // https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource
                var autoCompleteQueryEndpoint = serviceIndexObject["resources"]
                    .First(x => x["@type"].Value<string>() == "SearchAutocompleteService")["@id"]
                    .Value<string>();
                var autoCompleteQueryUrl = $"{autoCompleteQueryEndpoint}?q={keyword}&skip=0&take=5&prerelease=false&semVerLevel=2.0.0";
                var autoCompleteQueryResponse = await httpClient.GetStringAsync(autoCompleteQueryUrl);
                Console.WriteLine($"formatted autoCompleteQueryResponse:");
                Console.WriteLine($"{JObject.Parse(autoCompleteQueryResponse).ToString(Formatting.Indented)}");

                // https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource
                var packageVersionsEndpoint = serviceIndexObject["resources"]
                    .First(x => x["@type"].Value<string>() == "PackageBaseAddress/3.0.0")["@id"]
                    .Value<string>();

                var packageVersionsQueryUrl = $"{packageVersionsEndpoint}/weihanli.common/index.json";
                var packageVersionsQueryResponse = await httpClient.GetStringAsync(packageVersionsQueryUrl);
                Console.WriteLine("weihanli.common versions:");
                Console.WriteLine(JObject.Parse(packageVersionsQueryResponse).ToString(Formatting.Indented));
            }
        }
    }
}
