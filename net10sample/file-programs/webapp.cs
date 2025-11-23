#:sdk Microsoft.Net.Sdk.Web
var app = WebApplication.Create(args);
app.UseFileServer();
app.Run();
