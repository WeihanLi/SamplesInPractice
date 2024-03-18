using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;
using static WeihanLi.Common.Helpers.ConsoleHelper;

const string thisToolName = "update-all-tools";
const string thisToolId = $"dotnet-{thisToolName}";

if (args is { Length: 1 })
{
    // print help
    var helpArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "-h", "-?", "--help" };
    if (helpArguments.Contains(args[0]))
    {
        PrintHelp();
        return;
    }

    // print version
    var versionArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "-v", "--version", "version" };
    if (versionArguments.Contains(args[0]))
    {
        var libInfo = ApplicationHelper.GetLibraryInfo(typeof(Program));
        Console.WriteLine($"{libInfo.LibraryVersion} {libInfo.LibraryHash}");
        return;
    }
}

var isLocal = args is ["--local"];
var toolLevelOption = isLocal ? "--local" : "--global";
Console.WriteLine($"Going to update all {toolLevelOption.TrimStart('-')} tools");

var dotnetPath = Guard.NotNullOrEmpty(ApplicationHelper.GetDotnetPath());
var tools = await GetPackagesAsync();
foreach (var tool in tools)
{
    if (thisToolId.Equals(tool.PackageId))
        continue;

    var toolId = tool.PackageId;
    Console.WriteLine($"update tool {toolId}...");
    try
    {
        await RetryHelper.TryInvokeAsync(async () =>
        {
            var exitCode = await CommandExecutor.ExecuteAsync(dotnetPath, $"tool update {toolLevelOption} {toolId}",
                cancellationToken: ApplicationHelper.ExitToken);
            exitCode.EnsureSuccessExitCode();
        });
        Console.WriteLine($"update tool {toolId} completed");
    }
    catch (Exception ex)
    {
        WriteLineWithColor($"update tool {toolId} failed: {ex}", ConsoleColor.DarkRed);
    }
}

static void PrintHelp()
{
    Console.WriteLine("dotnet-update-all-tools is a tool for upgrade all dotnet tools, update all global dotnet tools by default, and you can set `--local` option for local tools");
    Console.WriteLine("Options:");
    Console.WriteLine($"\t {"--local",-20} \t update all local dotnet tools");
    Console.WriteLine($"\t {"-v/--version",-20} \t output version information");
    Console.WriteLine($"\t {"-h/--help",-20} \t output help information");
}

async Task<ToolListJsonContract[]> GetPackagesAsync()
{
#if NET9_0_OR_GREATER
    var jsonResult = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"tool list {toolLevelOption} --format=json", cancellationToken: ApplicationHelper.ExitToken);
    if (jsonResult.ExitCode is 0)
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<ToolListJsonWithVersionContract>(jsonResult.StandardOut);
        Guard.NotNull(result);
        if (result.Version != 1)
        {
            throw new NotSupportedException($"Version {result.Version} is not supported now");
        }
        return result.Data;
    }
#endif

    var tableResult = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"tool list {toolLevelOption}",
        cancellationToken: ApplicationHelper.ExitToken);
    var packages = tableResult.StandardOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        .Skip(2)
        .Select(line =>
        {
            var lineSplits = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return new ToolListJsonContract
            {
                PackageId = lineSplits[0], 
                Version = lineSplits[1],
                Commands = [lineSplits[2]]
            };
        }).ToArray();
    return packages;
}

[System.Diagnostics.DebuggerDisplay("{PackageId}, {Version}")]
internal sealed class ToolListJsonContract
{
    [System.Text.Json.Serialization.JsonPropertyName("packageId")]
    public required string PackageId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("version")]
    public required string Version { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("commands")]
    public required string[] Commands { get; init; }
}

#if NET9_0_OR_GREATER
internal sealed class ToolListJsonWithVersionContract
{
    /// <summary>
    /// The version of the JSON format for dotnet tool list.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public required ToolListJsonContract[] Data { get; init; }
}
#endif
