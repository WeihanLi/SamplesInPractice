using SignalRServerSample;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();

builder.Services.AddSignalR(options =>
{
#if DEBUG
    options.EnableDetailedErrors = true;
#endif
});

var app = builder.Build();

app.MapHub<HelloHub>("/hub/hello");

await app.RunAsync();
