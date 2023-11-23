using System.Text.Json;
using WeihanLi.Common.Helpers;
using static WeihanLi.Common.Helpers.ConsoleHelper;

const string toolName = "update-all-tools";
const string fullToolName = $"dotnet-{toolName}";

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
    var versionArguments = new HashSet<string>() { "-v", "--version", "version" };
    if (versionArguments.Contains(args[0]))
    {
        Console.WriteLine(JsonSerializer.Serialize(ApplicationHelper.GetLibraryInfo(typeof(Program))));
        return;
    }
}

var isLocal = args is ["--local"];
var toolLevelOption = isLocal ? "--local" : "--global";
Console.WriteLine($"Going to update all {toolLevelOption.TrimStart('-')} tools");

var exitToken = InvokeHelper.GetExitToken();
exitToken.Register(() => WriteLineWithColor("Exiting", ConsoleColor.DarkYellow));

var dotnetPath = ApplicationHelper.GetDotnetPath();
ArgumentNullException.ThrowIfNull(dotnetPath);
var dotnetToolListOutput = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, $"tool list {toolLevelOption}", cancellationToken: exitToken);
Console.WriteLine($"`dotnet tool list {dotnetToolListOutput}` output:");
Console.WriteLine(dotnetToolListOutput.StandardOut);

var dotnetToolList = dotnetToolListOutput.StandardOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
if (dotnetToolList.Length <= 2) return;

foreach (var tool in dotnetToolList[2..])
{
    if (exitToken.IsCancellationRequested) break;
    var toolId = tool.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
    if (fullToolName.Equals(toolId))
    {
        continue;
    }
    Console.WriteLine($"update tool {toolId}...");
    try
    {
        await CommandExecutor.ExecuteAsync(dotnetPath, $"tool update {toolLevelOption} {toolId}");
        Console.WriteLine($"update tool {toolId} completed");
    }
    catch (Exception ex)
    {
        WriteLineWithColor($"update tool {toolId} failed: {ex}", ConsoleColor.DarkRed);
    }
}

static void PrintHelp()
{
    Console.WriteLine("dotnet-update-all-tools is a tool for upgrade all dotnet tools, update all global dotnet tools by default");
    Console.WriteLine("Options");
    Console.WriteLine("`--local` \t\t You can set `--local` option to update all local dotnet tools");
    Console.WriteLine("`-v`/`--version` \t\t output version information");
    Console.WriteLine("`-h`/`--help` \t\t output help information");
}
