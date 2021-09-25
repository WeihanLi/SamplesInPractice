var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    options.RequestHeaders.Add("Cache-Control");
    options.ResponseHeaders.Add("Server");
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger().UseSwaggerUI();
app.UseHttpLogging();
app.MapControllers();

app.Run();
