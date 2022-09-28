using Microsoft.AspNetCore.Http.Json;

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
app.MapPost("/form-data-test", (HttpContext context) =>
{
    var name = context.Request.Form["name"].ToString();
    return Results.Ok(new { name });
});
app.Run();
