﻿var app = WebApplication.Create(args);
app.Map("/", () => "Hello World");
app.MapPost("/info", [HttpPost](IWebHostEnvironment env) => new 
{
    Time = DateTime.UtcNow, 
    env.EnvironmentName 
});
app.Run();
