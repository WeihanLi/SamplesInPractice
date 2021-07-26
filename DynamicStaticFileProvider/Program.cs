using System.Threading.Tasks;
using DynamicStaticFileProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<DynamicFileProviderOptions>(builder.Configuration);
builder.Services.AddSingleton<DynamicFileProvider>();
var app = builder.Build();
app.Map(new PathString("/redeploy"), appBuilder => appBuilder.Run(context =>
{
    if (context.RequestServices.GetRequiredService<IConfiguration>() is IConfigurationRoot configuration)
    {
        var currentSlot = configuration["CurrentSlot"];
        configuration["CurrentSlot"] = "Slot2" != currentSlot ? "Slot2" : "Slot1";
        configuration.Reload();
        return context.Response.WriteAsync("Success");
    }
    return Task.CompletedTask;
}));
var dynamicFileProvider = app.Services.GetRequiredService<DynamicFileProvider>();
app.UseStaticFiles(new StaticFileOptions {FileProvider = dynamicFileProvider});
app.Run();
