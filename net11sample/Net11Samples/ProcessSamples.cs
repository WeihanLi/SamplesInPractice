using System.Diagnostics;
using System.Text.Json;
using WeihanLi.Common.Helpers;

namespace Net11Samples;

internal static class ProcessSamples
{
    public static async Task MainTest()
    {
        {
            var result = Process.Run("dotnet", ["--version"]);
            ConsoleHelper.WriteLineWithColor(JsonSerializer.Serialize(result), ConsoleColor.Green);
            result = await Process.RunAsync("dotnet", ["--info"]);
            ConsoleHelper.WriteLineWithColor(JsonSerializer.Serialize(result), ConsoleColor.Green);

            result = await Process.RunAsync(new ProcessStartInfo("dotnet", "--info"), ApplicationHelper.ExitToken);
            ConsoleHelper.WriteLineWithColor(JsonSerializer.Serialize(result), ConsoleColor.Green);

            using var canceledCts = new CancellationTokenSource();
            canceledCts.Cancel();
            result = await Process.RunAsync(new ProcessStartInfo("dotnet", "--info"), canceledCts.Token);
            ConsoleHelper.WriteLineWithColor(JsonSerializer.Serialize(result), ConsoleColor.Green);
        }

        {
            var result = Process.RunAndCaptureText("dotnet", ["--version"]);
            ConsoleHelper.WriteLineWithColor(JsonSerializer.Serialize(result), ConsoleColor.Green);
            result = await Process.RunAndCaptureTextAsync("dotnet", ["--info"]);
            ConsoleHelper.WriteLineWithColor(JsonSerializer.Serialize(result), ConsoleColor.Green);
        }

        {
            var processId = Process.StartAndForget("dotnet", ["--version"]);
            Console.WriteLine($"ProcessId: {processId}");
        }

        {
            using var process = Process.Start(new ProcessStartInfo("dotnet", "--info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            Debug.Assert(process is not null);
            var output = process.ReadAllText();
            ConsoleHelper.WriteLineWithColor(output.StandardOutput, ConsoleColor.Green);
            ConsoleHelper.WriteLineWithColor(output.StandardError, ConsoleColor.Red);
        }

        {
            using var process = Process.Start(new ProcessStartInfo("dotnet", "--info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            Debug.Assert(process is not null);
            await foreach (var line in process.ReadAllLinesAsync())
            {
                ConsoleHelper.WriteLineWithColor(line.Content, line.StandardError ? ConsoleColor.Red : ConsoleColor.Green);
            }
        }
    }
}
