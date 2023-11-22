// https://github.com/dotnet/aspnetcore/issues/46280
// https://github.com/dotnet/aspnetcore/pull/47923

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
            // ExceptionHandlingPath = "/",
            ExceptionHandler = context =>
            {
                context.Response.StatusCode = 500;
                context.Response.WriteAsJsonAsync(new
                {
                    title = "Internal Error",
                    traceId = context.TraceIdentifier
                });
                return Task.CompletedTask;
            }
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
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.RequestServices.GetRequiredService<ILogger<ArgumentExceptionHandler>>()
            .LogError(exception, "Exception handled");
        if (exception is not ArgumentException) return false;

        httpContext.Response.StatusCode = 400;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            exception.Message
        }, cancellationToken);
        return true;
    }
}
