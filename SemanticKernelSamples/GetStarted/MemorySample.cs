using dotenv.net;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Plugins.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;

public static class MemorySample
{
    public static async Task MainTest()
    {
        DotEnv.Load();
        var apiEndpoint = Guard.NotNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_ENDPOINT"));
        var apiKey = Guard.NotNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));

        var memoryBuilder = new MemoryBuilder();
        memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
        memoryBuilder.WithMemoryStore(new Microsoft.SemanticKernel.Connectors.Memory.Redis.RedisMemoryStore("localhost:6379"));
        memoryBuilder.WithAzureTextEmbeddingGenerationService(Guard.NotNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_TEXT_EMBEDDING_DEPLOY_ID")), apiEndpoint, apiKey);
        var memory = memoryBuilder.Build();
        var collectionName = "Azure-Products";
        await using var fs = File.OpenRead("text-sample.json");
        var list = await JsonSerializer.DeserializeAsync<Product[]>(fs);
        Guard.NotNull(list);
        foreach (var product in list)
        {
            await memory.SaveInformationAsync(collectionName, product.Description, product.Title, product.Description, product.Category);
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
