namespace AspNetCore8Sample;

public class BasicSetupSample
{
    public static async Task MainTest(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddControllers();
        var app = builder.Build();
        app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            context.Response.Headers["Value"] = "123";
            await next(context);
        });
        app.MapGet("/", () => "Hello .NET 8!");
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-8.0#short-circuit-middleware-after-routing
        // https://github.com/dotnet/aspnetcore/issues/46071
        // https://github.com/dotnet/aspnetcore/pull/46713
        // ShortCircuit
        app.MapGet("/short-circuit", () => "Short circuiting!").ShortCircuit();
        app.MapGet("/short-circuit-status", () => "Short circuiting!")
            .ShortCircuit(401);
        // MapShortCircuit
        app.MapShortCircuit(404, "robots.txt", "favicon.ico");
        app.MapShortCircuit(403, "admin");
        
        app.MapControllers();
        await app.RunAsync();
    }
}
