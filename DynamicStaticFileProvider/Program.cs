using DynamicStaticFileProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureWebHostDefaults(webHostBuilder =>
{
    webHostBuilder.ConfigureServices((context, services) =>
    {
        services.Configure<DynamicFileProviderOptions>(context.Configuration);
        services.AddSingleton<DynamicFileProvider>();
    });
    webHostBuilder.Configure(app =>
    {
        app.Map(new PathString("/redeploy"), appBuilder => appBuilder.Run(context =>
        {
            if (context.RequestServices.GetRequiredService<IConfiguration>() is ConfigurationRoot configuration)
            {
                var currentSlot = configuration["CurrentSlot"];
                configuration["CurrentSlot"] = "Slot2" != currentSlot ? "Slot2" : "Slot1";
                configuration.Reload();
                return context.Response.WriteAsync("Success");
            }
            return Task.CompletedTask;
        }));

        var dynamicFileProvider = app.ApplicationServices.GetRequiredService<DynamicFileProvider>();
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = dynamicFileProvider,
        });
    });
});
var host = builder.Build();
host.Run();