using Microsoft.AspNetCore.HttpLogging;

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
        });
        builder.Services.AddHttpLoggingInterceptor<MyHttpLoggingInterceptor>();
        
        var app = builder.Build();
        app.UseHttpLogging();
        app.MapGet("/hello", () => "Hello");
        app.MapGet("/crash", () => Results.BadRequest());
        app.MapGet("/req-intercept", () => "Hello .NET 8");
        app.MapControllers();
        await app.RunAsync();
    }
}


file sealed class MyHttpLoggingInterceptor: IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        if (logContext.HttpContext.Request.Path.StartsWithSegments("/req-"))
        {
            logContext.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Duration;
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
