namespace GitHookSample;

public sealed class DeployHistory
{
    public required GithubPushEvent Event { get; init; }
    public DateTimeOffset BeginTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public TimeSpan Elapsed { get; set; }
}
