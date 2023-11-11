using GitHookSample;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;
using WeihanLi.Common.Event;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WebhookEventProcessor, MyWebhookEventProcessor>();
builder.Services.AddSingleton<GitHookSample.EventHandler>();
builder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<GitHookSample.EventHandler>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<GitHookSample.EventHandler>());

var app = builder.Build();
app.MapGitHubWebhooks("/api/github/webhooks",app.Configuration.GetRequiredAppSetting("GithubWebhookSecret"));
app.Map("/", () => "Hooks world");
await app.RunAsync();
