using System.Threading.Channels;
using WeihanLi.Common.Event;
using WeihanLi.Common.Helpers;

namespace GitHookSample;

public sealed class EventHandler : BackgroundService, IEventPublisher
{
    private readonly ILogger<EventHandler> _logger;
    private readonly IConfiguration _configuration;

    private readonly Channel<GithubPushEvent> _channel = 
        Channel.CreateBounded<GithubPushEvent>(new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.DropOldest 
        });

    public EventHandler(ILogger<EventHandler> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public bool Publish<TEvent>(TEvent @event) where TEvent : class, IEventBase
    {
        throw new NotImplementedException();
    }

    public async Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEventBase
    {
        if (@event is not GithubPushEvent githubPushEvent)
        {
            throw new NotSupportedException();            
        }
        
        await _channel.Writer.WriteAsync(githubPushEvent);
        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            if (_channel.Reader.TryRead(out var githubPushEvent))
            {
                var watch = ValueStopwatch.StartNew();
                try
                {
                    await HandleGithubPushEvent(githubPushEvent);
                    watch.Stop();
                    _logger.LogInformation("{RepoName} Deploy done, last commit msg: {CommitMsg}, {PushedBy}, please help check the result", 
                        githubPushEvent.RepoName, githubPushEvent.CommitMsg, githubPushEvent.PushByEmail);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{Method} Exception", nameof(HandleGithubPushEvent));
                }
            }

            await Task.Delay(10_000, stoppingToken);
        }
    }

    private async Task HandleGithubPushEvent(GithubPushEvent githubPushEvent)
    {
        // find repo, exec git pull
        var repoRoot = _configuration.GetRequiredAppSetting("RepoRoot");
        var repoFolder = Path.Combine(repoRoot, githubPushEvent.RepoName);
        if (!Directory.Exists(repoFolder))
        {
            throw new InvalidOperationException($"Repo({githubPushEvent.RepoName}) not exists in path {repoFolder}");
        }
        var pullExitCode = await CommandExecutor.ExecuteCommandAsync("git pull", repoFolder);
        if (pullExitCode != 0)
        {
            throw new InvalidOperationException($"Error when git pull, exitCode: {pullExitCode}");
        }
        
        // cleanup previous dist folder
        var distFolder = Path.Combine(repoFolder, "dist");
        Directory.Delete(distFolder, true);
        
        // exec yarn
        await CommandExecutor.ExecuteCommandAsync("yarn.cmd", repoFolder, info =>
        {
            info.EnvironmentVariables.Add("NODE_OPTIONS", "--openssl-legacy-provider");
        });
        
        // exec yarn build
        var result = await CommandExecutor.ExecuteAndCaptureAsync("yarn.cmd","build", repoFolder, info =>
        {
            info.EnvironmentVariables.Add("NODE_OPTIONS", "--openssl-legacy-provider");
        });
        if (result.ExitCode != 0)
        {
            _logger.LogError("Error when yarn build, exitCode: {ExitCode}, output: {Output}, error: {Error}",
                result.ExitCode, result.StandardOut, result.StandardError);
            throw new InvalidOperationException($"Error when yarn build, exitCode: {result.ExitCode}");
        }
        
        // copy dist to site folder
        var siteFolder = _configuration[$"AppSettings:RepoSiteMappings:{githubPushEvent.RepoName}"];
        if (string.IsNullOrEmpty(siteFolder))
        {
            _logger.LogError("No site name mapped, RepoName: {RepoName}", githubPushEvent.RepoName);
            throw new InvalidOperationException($"Error when yarn build, exitCode: {result.ExitCode}");
        }
        CopyDirectory(distFolder, siteFolder, true);
    }
    
    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (var subDir in dirs)
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
