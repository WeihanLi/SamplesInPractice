using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using WeihanLi.Common.Event;

namespace GitHookSample;

public sealed class MyWebhookEventProcessor: WebhookEventProcessor
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<MyWebhookEventProcessor> _logger;

    public MyWebhookEventProcessor(IEventPublisher eventPublisher, ILogger<MyWebhookEventProcessor> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }
    
    protected override async Task ProcessPushWebhookAsync(WebhookHeaders headers, PushEvent pushEvent)
    {
        var (repoName, repoFullName, commitId, commitMsg) = (pushEvent.Repository?.Name, pushEvent.Repository?.FullName, pushEvent.HeadCommit?.Id, pushEvent.HeadCommit?.Message);
        var (name, email) = (pushEvent.Pusher.Name, pushEvent.Pusher.Email);
        if (string.IsNullOrEmpty(commitId)
            || string.IsNullOrEmpty(commitMsg)
            || string.IsNullOrEmpty(repoName)
            || commitMsg.IndexOf("skip-ci", StringComparison.OrdinalIgnoreCase) > -1
            || commitMsg.IndexOf("skip-cd", StringComparison.OrdinalIgnoreCase) > -1
            || commitMsg.IndexOf("cd-skip", StringComparison.OrdinalIgnoreCase) > -1
           )
        {
            return;
        }
        
        _logger.LogInformation("Push event received {RepoName} {CommitId} {CommitMsg} {PushByName} {PushByEmail}",
            repoName, commitId, commitMsg, name, email);
        // process push event
        var githubPushEvent = new GithubPushEvent
        {
            RepoName = repoName,
            RepoFullName = repoFullName ?? repoName,
            CommitId = commitId,
            CommitMsg = commitMsg,
            Timestamp = DateTimeOffset.Parse(pushEvent.HeadCommit!.Timestamp),
            PushByName = name,
            PushByEmail = email ?? string.Empty
        };
        await _eventPublisher.PublishAsync(githubPushEvent);
    }
}
