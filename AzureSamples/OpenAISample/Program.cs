using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Extensions;
using OpenAI.Interfaces;
using OpenAISample;
using WeihanLi.Common;

// load .env
DotEnv.Fluent().Load();

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("openai-config.json")
    .AddEnvironmentVariables("OpenAISample_")
    .Build();
services.AddMemoryCache();
// services.RegisterOpenAIServices(configuration);

// OpenAI proxy
// services.AddOpenAIService(options =>
// {
//     options.ProviderType = ProviderType.OpenAi;
//     options.ApiKey = Guard.NotNull(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
//     options.BaseDomain = "https://aoai-proxy.weihanli.xyz";
// });

// Azure OpenAI, https://github.com/betalgo/openai/wiki/Azure-OpenAI
services.AddOpenAIService(options =>
{
    options.ProviderType = ProviderType.Azure;
    options.ApiKey = Guard.NotNull(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
    options.ResourceName = Guard.NotNull(Environment.GetEnvironmentVariable("AZURE_OPENAI_RES_NAME"));
    options.DeploymentId = Guard.NotNull(Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_ID"));
});
await using var applicationServices = services.BuildServiceProvider();

var openAIService = applicationServices.GetRequiredService<IOpenAIService>();

await ChatCompletionSample.MainTest(openAIService);
await EmbeddingSample.MainTest(openAIService);

Console.WriteLine("Hello, World!");
