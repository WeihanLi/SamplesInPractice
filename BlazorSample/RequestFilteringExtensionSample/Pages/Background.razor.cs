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
                // https://github.com/mjisaak/mvp-docs-learn-champion-extension/blob/master/src/main.js
                Urls = new[] 
                {
                    "https://social.technet.microsoft.com/*",
                    "https://docs.microsoft.com/*",
                    "https://azure.microsoft.com/*",
                    "https://techcommunity.microsoft.com/*",
                    "https://social.msdn.microsoft.com/*",
                    "https://devblogs.microsoft.com/*",
                    "https://developer.microsoft.com/*",
                    "https://channel9.msdn.com/*",
                    "https://gallery.technet.microsoft.com/*",
                    "https://cloudblogs.microsoft.com/*",
                    "https://technet.microsoft.com/*",
                    "https://docs.azure.cn/*",
                    "https://www.azure.cn/*",
                    "https://msdn.microsoft.com/*",
                    "https://blogs.msdn.microsoft.com/*",
                    "https://blogs.technet.microsoft.com/*",
                    "https://microsoft.com/handsonlabs/*",
                    "https://csc.docs.microsoft.com/*"
                }, 
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
