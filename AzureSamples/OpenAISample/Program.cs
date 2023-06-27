using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAISample;

// load .env
DotEnv.Fluent().Load();

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("openai-config.json")
    .AddEnvironmentVariables("OpenAISample")
    .Build();
services.RegisterOpenAIServices(configuration);
// services.AddOpenAIService(options =>
// {
//     options.ProviderType = ProviderType.OpenAi;
//     options.ApiKey = Guard.NotNull(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
//     options.BaseDomain = "https://aoai-proxy.weihanli.xyz";
// });
await using var applicationServices = services.BuildServiceProvider();

var openAIServiceFactory = applicationServices.GetRequiredService<IOpenAIServiceFactory>();
var openAIService = openAIServiceFactory.GetService("default");

// await ChatCompletionSample.MainTest(openAIService);
await EmbeddingSample.MainTest(openAIService);

Console.WriteLine("Hello, World!");
