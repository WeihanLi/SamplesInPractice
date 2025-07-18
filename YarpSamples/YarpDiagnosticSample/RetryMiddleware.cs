using Microsoft.Net.Http.Headers;
using System.Net;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.Transforms;

namespace YarpDiagnosticSample;

public sealed class RateLimitedRetryMiddleware(RequestDelegate next)
{
    private const int RetryLimit = 3;
    public const int StatusCodeToRetry = 429;
    
    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        var retryCount = 0;

        do
        {
            
            if (retryCount > 0)
            {
                if (context.Items.TryGetValue(HeaderNames.RetryAfter, out var retryAfter)
                    && int.TryParse(retryAfter?.ToString(), out var retryAfterSeconds))
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds));
                    context.Items.Remove(HeaderNames.RetryAfter);
                    context.Items[nameof(retryCount)] =  retryCount;
                }

                if (retryCount >= RetryLimit)
                {
                    context.Items["RetryLimited"] = "true";
                }
                
                // If this is a retry, we must reset the request body to initial position and clear the current response
                var reverseProxyFeature = context.GetReverseProxyFeature();
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                reverseProxyFeature.ProxiedDestination = null;
                context.Response.Clear();
            }

            await next(context);
            
            var statusCode = context.Response.StatusCode;
            if (statusCode is StatusCodeToRetry && retryCount < RetryLimit)
            {
                retryCount++;
            }
            else
            {
                break;
            }
        } while (true);
    }
}

public sealed class CustomHealthPolicy(IDestinationHealthUpdater healthUpdater)
    : IPassiveHealthCheckPolicy
{
    public string Name => nameof(CustomHealthPolicy);

    public void RequestProxied(HttpContext context, ClusterState cluster, DestinationState destination)
    {
        var headers = context.Response.Headers;
        if (context.Response.StatusCode is RateLimitedRetryMiddleware.StatusCodeToRetry)
        {
            var retryAfterSeconds = 1;

            if (headers.TryGetValue(HeaderNames.RetryAfter, out var retryAfterHeader) 
                && retryAfterHeader.Count == 1 
                && int.TryParse(retryAfterHeader[0], out var retryAfter)
                )
            {
                retryAfterSeconds = Math.Clamp(retryAfter, 1, 10);
            }
            
            context.Items[HeaderNames.RetryAfter] = retryAfterSeconds.ToString();
            healthUpdater.SetPassive(cluster, destination, DestinationHealth.Unhealthy, TimeSpan.FromSeconds(retryAfterSeconds));
        }
        else if (context.Response.StatusCode >= 500)
        {
            var reactivationInSeconds = 30;
            
            if (context.Items.TryGetValue("retryCount", out var retryCount)
                && int.TryParse(retryCount?.ToString(), out var retryCountValue))
            {
                var times = Math.Clamp(retryCountValue, 1, 3);
                reactivationInSeconds *= times;
            }
            
            healthUpdater.SetPassive(cluster, destination, DestinationHealth.Unhealthy, TimeSpan.FromSeconds(reactivationInSeconds));
        }
    }
}

public sealed class RateLimitRetryResponseTransform : ResponseTransform
{
    public override ValueTask ApplyAsync(ResponseTransformContext context)
    {
        if (!context.HttpContext.Items.ContainsKey("RetryLimited") && context.ProxyResponse is
            { StatusCode: (HttpStatusCode)RateLimitedRetryMiddleware.StatusCodeToRetry })
        {
            context.SuppressResponseBody = true;
        }
        return ValueTask.CompletedTask;
    }
}
