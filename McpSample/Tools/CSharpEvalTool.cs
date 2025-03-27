using ModelContextProtocol.Server;
using System.ComponentModel;
using WeihanLi.Common.Helpers;

namespace McpSample.Tools;

[McpServerToolType]
public static class CSharpEvalTool
{
    [McpServerTool, Description("Execute CSharp Code")]
    public static async Task<string> ExecCSharpCode(string code, CancellationToken cancellationToken)
    {
        var result = await CommandExecutor.ExecuteCommandAndCaptureAsync($"dotnet-exec {code}", cancellationToken: cancellationToken);
        return result.StandardOut;
    }
}
