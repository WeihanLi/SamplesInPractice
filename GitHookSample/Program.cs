using GitHookSample;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;
using System.Collections.Concurrent;
using WeihanLi.Common.Event;
using WeihanLi.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
var deployList = new ConcurrentQueue<DeployHistory>();

builder.Services.AddSingleton<WebhookEventProcessor, MyWebhookEventProcessor>();
builder.Services.AddSingleton<GitHookSample.EventHandler>();
builder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<GitHookSample.EventHandler>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<GitHookSample.EventHandler>());
builder.Services.AddSingleton(_ => deployList);

// configure for IIS
// https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/in-process-hosting?view=aspnetcore-6.0&WT.mc_id=DT-MVP-5004222#application-configuration
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});

var app = builder.Build();
app.MapGitHubWebhooks("/api/github/webhooks",app.Configuration.GetRequiredAppSetting("GithubWebhookSecret"));
app.Map("/", () => "Hooks world");
app.Map("/runtime-info", () => ApplicationHelper.RuntimeInfo);
app.Map("/deploy-history", () => deployList.ToArray());
app.MapPost("/deploy-test", async (IEventPublisher eventPublisher) =>
{
    var githubPushEvent = new GithubPushEvent
    {
        RepoName = "NetConfChina_Frontend",
        RepoFullName = "NetConfChina_Frontend",
        CommitId = "x",
        CommitMsg = "test",
        Timestamp = DateTimeOffset.Now,
        PushByName = "Test",
        PushByEmail = "weihanli@outlook.com"
    };
    await eventPublisher.PublishAsync(githubPushEvent);
});
await app.RunAsync();
