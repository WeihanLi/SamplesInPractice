var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Map("/", () => "Hello MinimalAPI").AddFilter<OutputDotNetVersionFilter>();

app.MapControllers();

app.Run();

internal sealed class OutputDotNetVersionFilter : IRouteHandlerFilter
{
    public async ValueTask<object?> InvokeAsync(RouteHandlerInvocationContext context, RouteHandlerFilterDelegate next)
    {
        context.HttpContext.Response.Headers["X-NET-Version"] = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        return await next(context);
    }
}
