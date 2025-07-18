using YarpDiagnosticSample;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddHttpLogging();
builder.Services.AddSingleton<YarpLoggingTransform>();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformContext =>
        transformContext.ResponseTransforms.Add(transformContext.Services.GetRequiredService<YarpLoggingTransform>()));
    ;
var app = builder.Build();
app.UseHttpLogging();
app.MapReverseProxy(proxyApp =>
{
    proxyApp.UseMiddleware<ResponseTextReplacementMiddleware>();
    proxyApp.UseMiddleware<ConditionalProxyMiddleware>();
    proxyApp.UseMiddleware<RateLimitedRetryMiddleware>();
    proxyApp.UsePassiveHealthChecks();
});
app.Run();
