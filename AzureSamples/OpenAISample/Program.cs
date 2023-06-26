using dotenv.net;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Extensions;
using OpenAI.Interfaces;
using OpenAISample;
using WeihanLi.Common;

// load .env
DotEnv.Fluent().Load();

var services = new ServiceCollection();
// services.AddOpenAIService(options =>
// {
//     options.ProviderType = ProviderType.Azure;
//     options.ApiKey = Guard.NotNull(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
//     options.ResourceName = Environment.GetEnvironmentVariable("AZURE_OPENAI_RESOURCE_NAME");
//     options.DeploymentId = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_ID");
//     options.ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2023-03-15-preview";
// });
services.AddOpenAIService(options =>
{
    options.ProviderType = ProviderType.OpenAi;
    options.ApiKey = Guard.NotNull(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
    options.BaseDomain = "https://aoai-proxy.weihanli.xyz";
});
await using var applicationServices = services.BuildServiceProvider();
var openAIService = applicationServices.GetRequiredService<IOpenAIService>();

// await ChatCompletionSample.MainTest(openAIService);
await EmbeddingSample.MainTest(openAIService);

Console.WriteLine("Hello, World!");
