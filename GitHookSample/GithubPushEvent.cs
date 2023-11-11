using WeihanLi.Common.Event;

namespace GitHookSample;

public sealed class GithubPushEvent : IEventBase
{
    public required string RepoName { get; init; }
    public required string RepoFullName { get; init; }
    
    public required string CommitId { get; init; }
    public required string CommitMsg { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    
    public required string PushByName { get; init; }
    public required string PushByEmail { get; init; }

    DateTimeOffset IEventBase.EventAt => Timestamp;

    string IEventBase.EventId => CommitId;
}
