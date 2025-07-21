using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

var client = await McpClientFactory.CreateAsync(new()
{
    Id = "aspnet-sample",
    Name = "ASP.NET McpServerSample",
    TransportType = TransportTypes.Sse,
    Location = "http://localhost:5000/sse",
});

// Print the list of tools available from the server.
foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"{tool.Name} ({tool.Description})");
}

var result = await client.CallToolAsync(
    "Echo",
    new Dictionary<string, object?> { ["message"] = "MCP!" },
    CancellationToken.None
    );

// echo always returns one and only one text content object
Console.WriteLine(result.Content.First(c => c.Type == "text").Text);
