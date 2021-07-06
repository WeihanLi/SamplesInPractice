using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using WebExtensions.Net.WebRequest;
using WeihanLi.Extensions;

namespace RequestFilteringExtensionSample.Pages
{
    public partial class Background
    {
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await WebExtensions.WebRequest.OnBeforeRequest.AddListener(info =>
            {
                var targetUrl = GetTargetUrl(info.Url);
                if (info.Url.Equals(targetUrl)) return new BlockingResponse();
                return new BlockingResponse {RedirectUrl = targetUrl};
            }, new RequestFilter
            {
                Urls = new[] {"*://docs.microsoft.com/*"}, 
                Types = new[] {ResourceType.MainFrame}
            }, new[] {OnBeforeRequestOptions.Blocking});
        }

        private const string TrackQueryKey = "WT.mc_id";
        private const string TrackQueryValue = "DT-MVP-5004222";

        private string GetTargetUrl(string originalUrl)
        {
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri))
            {
                return originalUrl;
            }
            try
            {
                var queryString = HttpUtility.ParseQueryString(uri.Query);
                if (TrackQueryValue == queryString.Get(TrackQueryKey))
                {
                    return originalUrl;
                }
                queryString.Set(TrackQueryKey, TrackQueryValue);
                var fullPath = $"{uri.Scheme}://{uri.Host}{uri.LocalPath}";
                var targetUrl =
                    $"{fullPath}?{queryString.AllKeys.Where(k => k.IsNotNullOrWhiteSpace()).Select(k => $"{k}={HttpUtility.UrlEncode(queryString[k])}").StringJoin("&")}";
                Logger.LogInformation($"original url: {originalUrl}, new url: {targetUrl}");
                return targetUrl;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception in {nameof(GetTargetUrl)}, {nameof(originalUrl)}:{originalUrl}");
                return originalUrl;
            }
        }
    }
}
