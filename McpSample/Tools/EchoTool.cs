using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpSample.Tools;

[McpToolType]
public static class EchoTool
{
    [McpTool, Description("Echoes the input back to the client.")]
    public static string Echo(string message)
    {
        return "hello " + message;
    }
}
