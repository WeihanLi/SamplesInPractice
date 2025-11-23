#:sdk Microsoft.Net.Sdk.Web
#:property ManagePackageVersionsCentrally=false
// #:property PublishAot=false
#:package WeihanLi.Web.Extensions@2.3.1-preview-20251123-015544
using WeihanLi.Web.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);
// var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.MapConfigInspector();
app.Run();
