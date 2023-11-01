using Microsoft.AspNetCore.HttpLogging;
using System.Diagnostics;

namespace AspNetCore8Sample;

public class HttpLoggingInterceptorSample
{
    public static async Task MainTest(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.All;
            // combine the request, request body, response, response body, and duration logs into a single log entry
            options.CombineLogs = true;
        });
        builder.Services.AddHttpLoggingInterceptor<MyHttpLoggingInterceptor>();
        
        var app = builder.Build();
        app.UseHttpLogging();
        app.MapGet("/no-log", () => "Hey .NET 8");
        app.MapGet("/hello", () => "Hello");
        app.MapGet("/crash", () => Results.BadRequest(new{ TraceId = Activity.Current?.TraceId.ToString() }));
        app.MapGet("/req-intercept", () => "Hello .NET 8");
        app.MapControllers();
        await app.RunAsync();
    }
}


file sealed class MyHttpLoggingInterceptor: IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        if ("/no-log".Equals(logContext.HttpContext.Request.Path.Value, StringComparison.OrdinalIgnoreCase))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }
        
        if (logContext.HttpContext.Request.Path.Value?.StartsWith("/req-") == true)
        {
            logContext.LoggingFields = HttpLoggingFields.ResponsePropertiesAndHeaders;
            logContext.AddParameter("req-path", logContext.HttpContext.Request.Path.Value);
        }
        
        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        if (logContext.HttpContext is { Response.StatusCode: >=200 and < 300, Request.Path.Value: "/hello" })
        {
            logContext.TryDisable(HttpLoggingFields.All);
        }
        return ValueTask.CompletedTask;
    }
}
