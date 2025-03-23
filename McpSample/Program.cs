using Azure;
using Azure.AI.Inference;
using McpSample;
using Microsoft.Extensions.AI;
using ModelContextProtocol;
using WeihanLi.Common;

// var builder = Host.CreateApplicationBuilder(args);

var builder = WebApplication.CreateSlimBuilder(args);
builder.Logging.AddDefaultDelegateLogger();

builder.Services.AddSingleton(new ChatCompletionsClient(
    new Uri("https://models.inference.ai.azure.com"),
    new AzureKeyCredential(builder.Configuration["GH_TOKEN"]!)
));
builder.Services.AddChatClient(sp => sp.GetRequiredService<ChatCompletionsClient>()
        .AsChatClient("gpt-4o-mini")
    )
    .UseLogging();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools()
    ;

var app = builder.Build();
DependencyResolver.SetDependencyResolver(app.Services);

// web application only
app.Map("/", () => "Hello McpServer");
app.MapMcpSse();

await app.RunAsync();
