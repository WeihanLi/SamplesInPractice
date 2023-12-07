using GitHookSample;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;
using WeihanLi.Common.Event;
using WeihanLi.Common.Helpers;
using WeihanLi.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebhookEventProcessor, MyWebhookEventProcessor>();
builder.Services.AddSingleton<GitHookSample.EventHandler>();
builder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<GitHookSample.EventHandler>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<GitHookSample.EventHandler>());
builder.Services.AddSingleton<IDeployHistoryRepository, DeployHistoryRepository>();

// configure for IIS
// https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/in-process-hosting?view=aspnetcore-6.0&WT.mc_id=DT-MVP-5004222#application-configuration
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});

var app = builder.Build();
app.MapGitHubWebhooks("/api/github/webhooks", app.Configuration.GetRequiredAppSetting("GithubWebhookSecret"));
app.Map("/", () => "Hooks world");
app.MapRuntimeInfo();
app.UseHealthCheck("/health", _ => Task.FromResult(true));

app.Map("/deploy-history", (IDeployHistoryRepository repository) => repository.GetAllDeployHistory());
app.Map("/deploy-history/{service}", (string service, IDeployHistoryRepository repository) => repository.GetDeployHistory(service));

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
