using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace AspNetCore8Sample;

// https://github.com/dotnet/aspnetcore/issues/23263
// https://github.com/dotnet/aspnetcore/pull/49249
public static class ForwardedHeadersSample
{
    public static async Task MainTest(string[] args)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true");
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
        });
        var app = builder.Build();
        app.MapGet("/", (HttpContext context) => new
        {
            ForwardedHeaders = context.RequestServices.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value
                .ForwardedHeaders.ToString(),
            ConnectIp = context.Connection.RemoteIpAddress?.ToString(),
            RequestHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Url = context.Request.GetDisplayUrl()
        });
        await app.RunAsync();
    }
}
