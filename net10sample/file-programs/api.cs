#:sdk Microsoft.Net.Sdk.Web
#:property ManagePackageVersionsCentrally=false
#:property PublishAot=false
#:package WeihanLi.Web.Extensions@*-*
using WeihanLi.Web.Extensions;

var app = WebApplication.Create(args);
app.MapGet("/", () => "Hello World!");
app.MapRuntimeInfo();
app.Run();
