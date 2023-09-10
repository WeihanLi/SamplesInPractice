using Microsoft.AspNetCore.Http.Timeouts;

namespace AspNetCore8Sample;

public static class RequestTimeoutsSample
{
    public static async Task MainTest(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new RequestTimeoutPolicy()
            {
                Timeout = TimeSpan.FromSeconds(1),
                WriteTimeoutResponse = context =>
                {
                    if (context.Response.HasStarted) return Task.CompletedTask;
                    return context.Response.WriteAsync("timeout");
                }
            };
            options.AddPolicy("timeout-2s", TimeSpan.FromSeconds(2));
        });
        var app = builder.Build();
        app.UseRequestTimeouts();
        app.Map("/", () => "Hello world");
        
        app.MapGet("/timeout", async (CancellationToken cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            return Results.Content("No timeout!", "text/plain");
        });
        app.MapGet("/timeout-policy", async (CancellationToken cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            return Results.Content("No timeout!", "text/plain");
        }).WithRequestTimeout("timeout-2s");
        app.MapGet("/timeout-attribute", [RequestTimeout(500)]async (CancellationToken cancellationToken) =>
        {
            try
            {
                await Task.Delay(800, cancellationToken);
            } 
            catch (TaskCanceledException)
            {
                return Results.Content("Timeout!", "text/plain");
            }
            return Results.Content("No timeout!", "text/plain");
        });
        
        app.MapGet("/no-timeout-attribute", [DisableRequestTimeout]async (CancellationToken cancellationToken) =>
        {
            try
            {
                await Task.Delay(5000, cancellationToken);
            } 
            catch (TaskCanceledException)
            {
                return Results.Content("Timeout!", "text/plain");
            }
            return Results.Content("No timeout!", "text/plain");
        });
        app.MapGet("/no-timeout", async (CancellationToken cancellationToken) =>
        {
            try
            {
                await Task.Delay(5000, cancellationToken);
            } 
            catch (TaskCanceledException)
            {
                return Results.Content("Timeout!", "text/plain");
            }
            return Results.Content("No timeout!", "text/plain");
        }).DisableRequestTimeout();

        app.MapGet("/special-timeout", async (CancellationToken cancellationToken) =>
        {
            await Task.Delay(1500, cancellationToken);
            return "Timeout";
        }).WithRequestTimeout(TimeSpan.FromSeconds(2));
        app.MapGet("/special-timeout-policy", async (CancellationToken cancellationToken) =>
        {
            await Task.Delay(1500, cancellationToken);
            return "Timeout";
        }).WithRequestTimeout(new RequestTimeoutPolicy()
        {
            Timeout = TimeSpan.FromSeconds(1),
            WriteTimeoutResponse = (context) => context.Response.WriteAsync("Oh it's timeout")
        });
        app.MapGet("/special-timeout-policy", async (CancellationToken cancellationToken) =>
        {
            await Task.Delay(1500, cancellationToken);
            return "Timeout";
        }).WithRequestTimeout("timeout-2s");

        app.MapGet("/timeout-sync",  () =>
        {
            Thread.Sleep(5000);
            return Results.Content("No timeout!", "text/plain");
        });

        await app.RunAsync();
    }
}
