var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
builder.Services.AddRoutingCore();
builder.WebHost.UseKestrelCore();
// builder.Logging.AddConsole();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints => 
{
    endpoints.MapGet("/", () => "Hello World");
});

app.Run();
