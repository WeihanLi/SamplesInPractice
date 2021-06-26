var app = WebApplication.Create(args);
app.Map("/", () => "Hello World");
app.Map("/info", [HttpPost]() => new{ Time = DateTime.UtcNow});
app.Run();
