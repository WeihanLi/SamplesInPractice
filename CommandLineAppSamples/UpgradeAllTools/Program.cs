﻿using System.CommandLine;
using WeihanLi.Common.Helpers;

const string toolName = "update-all-tools";
const string fullToolName = $"dotnet-{toolName}";

var command = new Command(toolName);
command.SetHandler(async () =>
{
    var dotnetPath = ApplicationHelper.GetDotnetPath();
    var dotnetToolListOutput = await CommandExecutor.ExecuteAndCaptureAsync(dotnetPath, "tool list -g");
    Console.WriteLine("`dotnet tool list -g` output:");
    Console.WriteLine(dotnetToolListOutput.StandardOut);

    var dotnetToolList = dotnetToolListOutput.StandardOut.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    if (dotnetToolList.Length > 2)
    {
        foreach (var tool in dotnetToolList[2..])
        {
            var toolId = tool.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            if (fullToolName.Equals(toolId))
            {
                continue;
            }
            Console.WriteLine($"update tool {toolId}...");
            try
            {
                await CommandExecutor.ExecuteAsync(dotnetPath, $"tool update -g {toolId}");
                Console.WriteLine($"update tool {toolId} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"update tool {toolId} failed: {ex}");
            }
        }
    }
});
await command.InvokeAsync(args);
