var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Map("/", () => "Hello MinimalAPI").AddRouteHandlerFilter<OutputDotNetVersionFilter>();

var hello = app.MapGroup("/hello");
hello.Map("/test", () => "test");
hello.Map("/test2", () => "test2");

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
