using System;
using System.Collections.Specialized;
using System.IO;

namespace MiniAspNetCore
{
    public class HttpRequest
    {
        private readonly IRequestFeature _requestFeature;

        public HttpRequest(IFeatureCollection featureCollection)
        {
            _requestFeature = featureCollection.Get<IRequestFeature>();
        }

        public Uri Url => _requestFeature.Url;

        public NameValueCollection Headers => _requestFeature.Headers;

        public string Method => _requestFeature.Method;

        public string Host => _requestFeature.Url.Host;

        public Stream Body => _requestFeature.Body;
    }
}
