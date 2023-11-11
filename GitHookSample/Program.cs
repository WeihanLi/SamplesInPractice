using GitHookSample;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;
using WeihanLi.Common.Event;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WebhookEventProcessor, MyWebhookEventProcessor>();
builder.Services.AddSingleton<GitHookSample.EventHandler>();
builder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<GitHookSample.EventHandler>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<GitHookSample.EventHandler>());

// configure for IIS
// https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/in-process-hosting?view=aspnetcore-6.0&WT.mc_id=DT-MVP-5004222#application-configuration
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});

var app = builder.Build();
app.MapGitHubWebhooks("/api/github/webhooks",app.Configuration.GetRequiredAppSetting("GithubWebhookSecret"));
app.Map("/", () => "Hooks world");
await app.RunAsync();
