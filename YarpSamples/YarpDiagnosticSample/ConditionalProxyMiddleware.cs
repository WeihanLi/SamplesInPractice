namespace YarpDiagnosticSample;

public class ConditionalProxyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (await WhetherToProxy(context))
        {
            await next(context);
        }
        else
        {
            // internal handling
            await context.Response.WriteAsync("Hello World! No proxy here");
        }
    }

    private static Task<bool> WhetherToProxy(HttpContext context)
    {
        if (context.Request.Query.ContainsKey("no-proxy"))
        {
            return Task.FromResult(false);
        }
        
        // feature flag integration
        
        return Task.FromResult(true);
    }
}
