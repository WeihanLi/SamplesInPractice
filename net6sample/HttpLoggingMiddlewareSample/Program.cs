var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpLogging(options =>
{
    // options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    options.RequestHeaders.Add("Cache-Control");
    options.ResponseHeaders.Add("Server");
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpLogging();
app.MapControllers();

app.Run();
