// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.SignalR.Client;
using SignalRSample;
using System.Text.Json;
using TypedSignalR.Client;

await using var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5069/hub/hello")
    .Build();

connection.Reconnected += c =>
{
    Console.WriteLine($"reconnected, connectionId: {c}");
    return Task.CompletedTask;
};

await connection.StartAsync();
connection.Register<IHelloClient>(new HubClient());
var hub = connection.CreateHubProxy<IHelloServer>();
await hub.Hello(new HelloModel() { Name = "test-client", Message = "Hello SignalR" });
Console.Read();

public sealed class HubClient : IHelloClient
{
    public Task Hello(HelloModel model)
    {
        Console.WriteLine($"{nameof(HubClient)}.{nameof(Hello)}");
        Console.WriteLine(JsonSerializer.Serialize(model));
        return Task.CompletedTask;
    }
}
