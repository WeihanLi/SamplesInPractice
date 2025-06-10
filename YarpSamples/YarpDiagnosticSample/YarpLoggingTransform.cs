using Yarp.ReverseProxy.Transforms;

namespace YarpDiagnosticSample;

public class YarpLoggingTransform : ResponseTransform
{
    public override ValueTask ApplyAsync(ResponseTransformContext context)
    {
        var proxyResponse = context.ProxyResponse;
        if (proxyResponse is null)
            return ValueTask.CompletedTask;
        
        var proxyRequest = proxyResponse.RequestMessage;

        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<YarpLoggingTransform>>();
        if (proxyRequest is not null)
        {
            logger.LogInformation("Proxy request: {Url} {Headers}", 
                proxyRequest.RequestUri?.AbsoluteUri, string.Join(";",
                    proxyRequest.Headers.Select(h => 
                        $"{h.Key}:{string.Join(",", h.Value)}")));
        }
        
        logger.LogInformation("Proxy response: {ResponseStatusCode} {ResponseHeaders}",
            (int)proxyResponse.StatusCode, string.Join(";", proxyResponse.Headers.Select(x=> $"{x.Key}:{string.Join(",", x.Value)}")));
        return ValueTask.CompletedTask;
    }
}
