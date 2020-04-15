using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace MiniAspNetCore
{
    public interface IRequestFeature
    {
        Uri Url { get; }

        string Method { get; }

        NameValueCollection Headers { get; }

        Stream Body { get; }
    }

    public interface IResponseFeature
    {
        public int StatusCode { get; set; }

        NameValueCollection Headers { get; set; }

        public Stream Body { get; }
    }

    public class HttpListenerFeature : IRequestFeature, IResponseFeature
    {
        private readonly HttpListenerContext _httpListenerContext;

        public HttpListenerFeature(HttpListenerContext httpListenerContext)
        {
            _httpListenerContext = httpListenerContext;
        }

        int IResponseFeature.StatusCode { get => _httpListenerContext.Response.StatusCode; set => _httpListenerContext.Response.StatusCode = value; }

        NameValueCollection IResponseFeature.Headers
        {
            get => _httpListenerContext.Response.Headers;
            set
            {
                _httpListenerContext.Response.Headers = new WebHeaderCollection();
                foreach (var key in value.AllKeys)
                    _httpListenerContext.Response.Headers.Add(key, value[key]);
            }
        }

        NameValueCollection IRequestFeature.Headers => _httpListenerContext.Request.Headers;

        Stream IResponseFeature.Body => _httpListenerContext.Response.OutputStream;

        Stream IRequestFeature.Body => _httpListenerContext.Request.InputStream;

        string IRequestFeature.Method => _httpListenerContext.Request.HttpMethod;

        Uri IRequestFeature.Url => _httpListenerContext.Request.Url;
    }
}
