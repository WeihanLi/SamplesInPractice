using Microsoft.AspNetCore.Diagnostics;

namespace AspNetCore8Sample;

public static class ExceptionHandlerSample
{
    public static async Task MainTest(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddExceptionHandler<ArgumentExceptionHandler>();
        var app = builder.Build();

        app.UseExceptionHandler(new ExceptionHandlerOptions()
        {
            ExceptionHandlingPath = "/"
        });
        app.MapGet("/", () => "Hello .NET 8!");
        app.MapGet("/exception", () =>
        {
            throw new InvalidOperationException("Oh no...");
        });
        app.MapGet("/argument-exception", () =>
        {
            throw new ArgumentException("Oh no...");
        });
        await app.RunAsync();
    }
}


file sealed class ArgumentExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.RequestServices.GetRequiredService<ILogger<ArgumentExceptionHandler>>()
            .LogError(exception, "Exception handled");
        return ValueTask.FromResult(exception is ArgumentException);
    }
}
