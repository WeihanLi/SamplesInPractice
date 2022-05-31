using System;
using System.Threading.Tasks;
using WeihanLi.Extensions;

namespace MiniAspNetCore
{
    public class HttpResponse
    {
        private readonly IResponseFeature _responseFeature;

        public HttpResponse(IFeatureCollection featureCollection)
        {
            _responseFeature = featureCollection.Get<IResponseFeature>();
        }

        public bool ResponseStarted => _responseFeature.Body.Length > 0;

        public int StatusCode
        {
            get => _responseFeature.StatusCode;
            set => _responseFeature.StatusCode = value;
        }

        public async Task WriteBytesAsync(byte[] responseBytes)
        {
            if (_responseFeature.StatusCode <= 0)
            {
                _responseFeature.StatusCode = 200;
            }
            if (responseBytes != null && responseBytes.Length > 0)
            {
                await _responseFeature.Body.WriteAsync(responseBytes);
            }
        }
    }

    public static class HttpResponseExtensions
    {
        public static Task WriteAsync(this HttpResponse response, string responseText)
            => string.IsNullOrEmpty(responseText) ? Task.CompletedTask : response.WriteBytesAsync(responseText.GetBytes());

        public static Task WriteLineAsync(this HttpResponse response, string responseText)
            => string.IsNullOrEmpty(responseText) ? Task.CompletedTask : response.WriteBytesAsync($"{responseText}{Environment.NewLine}".GetBytes());
    }
}
