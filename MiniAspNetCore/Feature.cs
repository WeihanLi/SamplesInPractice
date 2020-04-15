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

    public class HttpListenerRequestFeature : IRequestFeature
    {
        private readonly HttpListenerRequest _request;

        public HttpListenerRequestFeature(HttpListenerContext listenerContext)
        {
            _request = listenerContext.Request;
        }

        public Uri Url => _request.Url;
        public string Method => _request.HttpMethod;
        public NameValueCollection Headers => _request.Headers;
        public Stream Body => _request.InputStream;
    }

    public class HttpListenerResponseFeature : IResponseFeature
    {
        private readonly HttpListenerResponse _response;

        public HttpListenerResponseFeature(HttpListenerContext httpListenerContext)
        {
            _response = httpListenerContext.Response;
        }

        public int StatusCode { get => _response.StatusCode; set => _response.StatusCode = value; }

        public NameValueCollection Headers
        {
            get => _response.Headers;
            set
            {
                _response.Headers = new WebHeaderCollection();
                foreach (var key in value.AllKeys)
                    _response.Headers.Add(key, value[key]);
            }
        }

        public Stream Body => _response.OutputStream;
    }

    public static class HttpListenerContextExtensions
    {
        public static IRequestFeature GetRequestFeature(this HttpListenerContext context)
        {
            return new HttpListenerRequestFeature(context);
        }

        public static IResponseFeature GetResponseFeature(this HttpListenerContext context)
        {
            return new HttpListenerResponseFeature(context);
        }
    }
}
