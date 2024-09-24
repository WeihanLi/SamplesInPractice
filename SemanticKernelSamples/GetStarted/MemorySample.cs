using dotenv.net;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Microsoft.SemanticKernel.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;

namespace GetStarted;

public static class MemorySample
{
    public static async Task MainTest()
    {
        DotEnv.Load();
        var apiEndpoint = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_ENDPOINT"));
        var deployment = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_TEXT_EMBEDDING_DEPLOY_ID"));
        var apiKey = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_KEY"));
        var mongoDbConn = Guard.NotNullOrEmpty(EnvHelper.Val("MONGODB_CONNECTION"));

        var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddDefaultDelegateLogger());
        var memoryBuilder = new MemoryBuilder();
        // memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
        memoryBuilder.WithMemoryStore(new MongoDBMemoryStore(mongoDbConn, "playground", "embedding"));
        memoryBuilder.WithTextEmbeddingGeneration(new AzureOpenAITextEmbeddingGenerationService(
            deployment, apiEndpoint, apiKey,
            "text-embedding-small-003", null, loggerFactory, 256)
        );
        var memory = memoryBuilder.Build();
        
        var collectionName = "AzureProducts";
                
        await using var fs = File.OpenRead("text-sample.json");
        var list = await JsonSerializer.DeserializeAsync<Product[]>(fs);
        Guard.NotNull(list);
        var productFromMemory = await memory.GetAsync(collectionName, list[0].Title);
        if (productFromMemory is null)
        {
            foreach (var product in list)
            {
                await memory.SaveInformationAsync(collectionName, product.Description, product.Title, product.Description, product.Category);
                Console.WriteLine($"[{product.Title}] saved");
            }    
        }

        await ConsoleHelper.HandleInputLoopAsync(async input =>
        {
            var result = memory.SearchAsync(collectionName, input, 3, 0.8);
            await foreach (var item in result)
            {
                Console.WriteLine($"{nameof(item.Relevance)}: {item.Relevance}");
                Console.WriteLine(item.Metadata.Id);
                Console.WriteLine(item.Metadata.Text);
                Console.WriteLine();
            }
        }, "Input your questions");
    }
}

file sealed class Product
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    [JsonPropertyName("category")]
    public required string Category { get; set; }
    
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("content")]
    public required string Description { get; set; }
}
