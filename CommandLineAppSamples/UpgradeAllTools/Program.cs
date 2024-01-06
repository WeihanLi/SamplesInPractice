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

var exitToken = InvokeHelper.GetExitToken();
var dotnetPath = ApplicationHelper.GetDotnetPath();
ArgumentNullException.ThrowIfNull(dotnetPath);
var dotnetToolListOutput = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"tool list {toolLevelOption}", cancellationToken: exitToken);
Console.WriteLine($"`dotnet tool list {toolLevelOption}` output:");
Console.WriteLine(dotnetToolListOutput.StandardOut);

var dotnetToolList = dotnetToolListOutput.StandardOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
if (dotnetToolList.Length <= 2) return;

foreach (var tool in dotnetToolList[2..])
{
    var toolId = tool.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
    if (thisToolId.Equals(toolId))
        continue;

    Console.WriteLine($"update tool {toolId}...");
    try
    {
        await RetryHelper.TryInvokeAsync(async () =>
        {
            var exitCode = await CommandExecutor.ExecuteAsync(dotnetPath, $"tool update {toolLevelOption} {toolId}",
                cancellationToken: exitToken);
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
