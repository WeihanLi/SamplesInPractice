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

        public Task WriteAsync(byte[] responseBytes)
        {
            if (_responseFeature.StatusCode <= 0)
            {
                _responseFeature.StatusCode = 200;
            }

            return _responseFeature.Body.WriteAsync(responseBytes).AsTask();
        }
    }

    public static class HttpResponseExtensions
    {
        public static Task WriteAsync(this HttpResponse response, string responseText)
            => response.WriteAsync(responseText.GetBytes());
    }
}
