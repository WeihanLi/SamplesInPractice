namespace YarpDiagnosticSample;

public class ResponseTextReplacementMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var whetherToReplace = WhetherToRelace(context);
        if (!whetherToReplace)
        {
            await next(context);
            return;
        }
        
        // https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/HttpLogging/src/HttpLoggingMiddleware.cs#L204
        var originalResponseStream = context.Response.Body;
        
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;
        
        await next(context);
        
        context.Response.Body = originalResponseStream;
        
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream);
        var responseText = await reader.ReadToEndAsync(context.RequestAborted);
        var replacedText = responseText.Replace("World", ".NET");
        if (replacedText == responseText)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        else
        {
            await context.Response.WriteAsync(replacedText, context.RequestAborted);
        }
    }

    private static bool WhetherToRelace(HttpContext context)
    {
        // feature flag integration
        return true;
    }
}
