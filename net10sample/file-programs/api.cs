#:sdk Microsoft.Net.Sdk.Web
#:property ManagePackageVersionsCentrally=false
#:property PublishAot=false
#:package WeihanLi.Web.Extensions@2.1.0
using WeihanLi.Web.Extensions;

var app = WebApplication.Create(args);
app.MapGet("/", () => "Hello World!");
app.MapRuntimeInfo().ShortCircuit();
app.MapConfigInspector();
app.Run();
