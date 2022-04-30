using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});
var app = builder.Build();
app.Map("/", () => "Hello World");
app.MapPost("/info", (IWebHostEnvironment env) => new
{
    Time = DateTime.UtcNow,
    env.EnvironmentName
});
app.Run();
