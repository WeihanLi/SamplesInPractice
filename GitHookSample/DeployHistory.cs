namespace GitHookSample;

public class DeployHistory
{
    public required GithubPushEvent Event { get; init; }
    public DateTimeOffset BeginTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}
