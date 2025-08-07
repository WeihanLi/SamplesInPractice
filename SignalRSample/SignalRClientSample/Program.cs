using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using SignalRSample;
using System.Net;
using System.Text.Json;
using TypedSignalR.Client;

await using var connection = new HubConnectionBuilder()
    .ConfigureLogging(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug).AddDefaultDelegateLogger())
    // .WithUrl("https://localhost:5002/hub/hello", options =>
    // .WithUrl("http://localhost:5001/hub/hello", options =>
    .WithUrl("http://localhost:5000/hub/hello", options =>
    {
        options.SkipNegotiation = true;
        options.Transports = HttpTransportType.WebSockets;
        options.HttpMessageHandlerFactory = _ => new SocketsHttpHandler();
        options.WebSocketConfiguration = clientWebSocketOptions =>
        {
            clientWebSocketOptions.Proxy = new WebProxy("http://localhost:8000");
            // clientWebSocketOptions.HttpVersion = HttpVersion.Version20;
            // clientWebSocketOptions.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        };
    })
    .Build();
connection.Reconnected += c =>
{
    Console.WriteLine($"reconnected, connectionId: {c}");
    return Task.CompletedTask;
};

await connection.StartAsync();
connection.Register<IHelloClient>(new HubClient());
var hub = connection.CreateHubProxy<IHelloServer>();
await hub.Hello(new HelloModel { Name = "test-client", Message = "Hello SignalR" });
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
