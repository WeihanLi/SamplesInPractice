﻿using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServerSample.Tools;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the input back to the client.")]
    public static string Echo(string message)
    {
        return "hello " + message;
    }
}
