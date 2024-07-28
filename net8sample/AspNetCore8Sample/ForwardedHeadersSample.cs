using Microsoft.AspNetCore.Http.Extensions;

namespace AspNetCore8Sample;

public static class ForwardedHeadersSample
{
    public static async Task MainTest(string[] args)
    {
        // Environment.SetEnvironmentVariable("ASPNETCORE_USEFORWARDEDHEADHERS_ENABLED", "true");
        var app = WebApplication.Create(args);
        app.Map("/", (HttpContext context) => new
        {
            Url = context.Request.GetDisplayUrl()
        });
        await app.RunAsync();
    }
}
