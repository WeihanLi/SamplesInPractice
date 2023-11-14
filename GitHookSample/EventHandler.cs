using LibGit2Sharp;
using System.Threading.Channels;
using WeihanLi.Common.Event;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace GitHookSample;

public sealed class EventHandler : BackgroundService, IEventPublisher
{
    private readonly ILogger<EventHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDeployHistoryRepository _deployHistoryRepository;

    private readonly Channel<GithubPushEvent> _channel = 
        Channel.CreateBounded<GithubPushEvent>(new BoundedChannelOptions(3)
        {
            FullMode = BoundedChannelFullMode.DropOldest 
        });

    public EventHandler(ILogger<EventHandler> logger, IConfiguration configuration, IDeployHistoryRepository deployHistoryRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _deployHistoryRepository = deployHistoryRepository;
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
        await foreach (var githubPushEvent in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            var beginTime = DateTimeOffset.Now;
            var watch = ValueStopwatch.StartNew();
            try
            {
                await HandleGithubPushEvent(githubPushEvent);
                watch.Stop();
                var endTime = DateTimeOffset.Now;
                _logger.LogInformation("{RepoName} Deploy done in {Elapsed}, last commit msg: {CommitMsg}, {PushedBy}, please help check the result", 
                    githubPushEvent.RepoName, watch.Elapsed, githubPushEvent.CommitMsg, githubPushEvent.PushByEmail);
                var deployHistory = new DeployHistory
                {
                    Event = githubPushEvent, 
                    BeginTime = beginTime,
                    EndTime = endTime,
                    Elapsed = watch.Elapsed
                };
                _deployHistoryRepository.AddDeployHistory(githubPushEvent.RepoName, deployHistory);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Method} Exception", nameof(HandleGithubPushEvent));
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

        if (_configuration.GetAppSetting("PreferLibGit2Sharp", false))
        {
            using var repo = new Repository(repoFolder);
            // Credential information to fetch
            var options = new PullOptions 
            { 
                FetchOptions = new FetchOptions 
                { 
                    CredentialsProvider = (_, _, _) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = _configuration["GitCredential:Name"],
                            Password = _configuration["GitCredential:Token"]
                        }
                } };
        
            // User information to create a merge commit
            var signature = new Signature(new Identity(_configuration["GitCredential:Name"], _configuration["GitCredential:Email"]), DateTimeOffset.Now);
        
            // Pull
            RetryHelper.TryInvoke(() => Commands.Pull(repo, signature, options), 10);
        }
        else
        {
            var gitPath = ApplicationHelper.ResolvePath("git") ?? _configuration.GetRequiredAppSetting("GitPath");
            var gitPullResult = await RetryHelper.TryInvokeAsync(() => CommandExecutor.ExecuteAndCaptureAsync(gitPath, "pull", repoFolder),
                r => r?.ExitCode == 0, 10);
            if (gitPullResult?.ExitCode != 0)
            {
                throw new InvalidOperationException($"Error when git pull, exitCode: {gitPullResult?.ExitCode}, {gitPullResult?.StandardError}");
            }
        }
        
        var nodePath = _configuration.GetRequiredAppSetting("NodePath");
        var previousPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var updatedPath = previousPath.EndsWith(';') ? $"{previousPath}{nodePath}" : $"{previousPath};{nodePath}";
        _logger.LogInformation("Previous environment path: {PreviousPath}, updatedPath: {UpdatedPath}", previousPath, updatedPath);
        var yarnPath = ApplicationHelper.ResolvePath("yarn.cmd") ?? _configuration.GetRequiredAppSetting("YarnPath");
        // exec yarn
        var yarnResult = await CommandExecutor.ExecuteAndCaptureAsync(yarnPath, null, repoFolder, info =>
        {
            if (_configuration.GetAppSetting<bool>("AddNodeOptionsEnv"))
                info.EnvironmentVariables.Add("NODE_OPTIONS", "--openssl-legacy-provider");
            
            info.EnvironmentVariables["NODE_PATH"] = nodePath;
            info.EnvironmentVariables["PATH"] = updatedPath;
            
            var processUser = _configuration["ProcessUserCredential:UserName"];
            if (!string.IsNullOrEmpty(processUser))
            {
                info.UserName = processUser;
                if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(_configuration["ProcessUserCredential:Password"]))
                  info.PasswordInClearText = Convert.FromBase64String(_configuration["ProcessUserCredential:Password"]).GetString();
            }
        });
        if (yarnResult.ExitCode != 0)
        {
            _logger.LogError("Error when yarn, exitCode: {ExitCode}, output: {Output}, error: {Error}",
                yarnResult.ExitCode, yarnResult.StandardOut, yarnResult.StandardError);
            throw new InvalidOperationException($"Error when yarn, exitCode: {yarnResult.ExitCode}");
        }
        
        // cleanup previous dist folder
        var distFolder = Path.Combine(repoFolder, "dist");
        if (Directory.Exists(distFolder))
            Directory.Delete(distFolder, true);
        
        // exec yarn build
        var buildResult = await CommandExecutor.ExecuteAndCaptureAsync(yarnPath, "build", repoFolder, info =>
        {
            if (_configuration.GetAppSetting<bool>("AddNodeOptionsEnv"))
                info.EnvironmentVariables.Add("NODE_OPTIONS", "--openssl-legacy-provider");
            
            info.EnvironmentVariables["NODE_PATH"] = nodePath;
            info.EnvironmentVariables["PATH"] = updatedPath;
            
            var processUser = _configuration["ProcessUserCredential:UserName"];
            if (!string.IsNullOrEmpty(processUser))
            {
                info.UserName = processUser;
                if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(_configuration["ProcessUserCredential:Password"]))
                    info.PasswordInClearText = Convert.FromBase64String(_configuration["ProcessUserCredential:Password"]).GetString();
            }
        });
        if (buildResult.ExitCode != 0)
        {
            _logger.LogError("Error when yarn build, exitCode: {ExitCode}, output: {Output}, error: {Error}",
                buildResult.ExitCode, buildResult.StandardOut, buildResult.StandardError);
            throw new InvalidOperationException($"Error when yarn build, exitCode: {buildResult.ExitCode}");
        }
        
        // copy dist to site folder
        var siteFolder = _configuration[$"AppSettings:RepoSiteMappings:{githubPushEvent.RepoName}"];
        if (string.IsNullOrEmpty(siteFolder))
        {
            _logger.LogError("No site name mapped, RepoName: {RepoName}", githubPushEvent.RepoName);
            throw new InvalidOperationException($"Error when yarn build, exitCode: {buildResult.ExitCode}");
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
            file.CopyTo(targetFilePath, true);
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
