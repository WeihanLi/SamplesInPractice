using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Numerics.Tensors;
using System.Text.Json;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace GetStarted;

public static class DotnetConfHelper
{
    public static async Task MainTest()
    {
        var chatDeployId = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_CHAT_COMPLETION_DEPLOY_ID"));
        var embeddingDeployId = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_TEXT_EMBEDDING_DEPLOY_ID"));
        var apiEndpoint = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_ENDPOINT"));
        var apiKey = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_KEY"));

        var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddDefaultDelegateLogger());
        var textEmbeddingService = new AzureOpenAITextEmbeddingGenerationService(embeddingDeployId, apiEndpoint, apiKey, loggerFactory: loggerFactory);
        
        var filePath = "dotnetconf2024-agenda.json";
        var filePathWithEmbeddings = "dotnetconf2024-agenda-with-embeddings.json";
        Session[] sessions;
        if (File.Exists(filePathWithEmbeddings))
        {
            await using var stream = File.OpenRead(filePathWithEmbeddings);
            sessions = Guard.NotNull(await JsonSerializer.DeserializeAsync<Session[]>(stream));
        }
        else
        {
            {
                await using var stream = File.OpenRead(filePath);
                sessions = Guard.NotNull(await JsonSerializer.DeserializeAsync<Session[]>(stream));
                foreach (var session in sessions)
                {
                    var embeddings = await textEmbeddingService.GenerateEmbeddingsAsync
                    ([
                        $"""
                         {session.Title}
                         {session.Description}
                         """
                    ]).ContinueWith(r => r.Result[0]);
                    session.Embeddings = embeddings.ToArray();
                    Console.WriteLine($"{session.Title} embeddings generated");
                }
            }
            {
                await using var fs = File.Create(filePathWithEmbeddings);
                await JsonSerializer.SerializeAsync(fs, sessions);
            }
        }
        
        var mlContext = new MLContext(seed: 0);

        // Create a list of training data points.
        var trainingData = mlContext.Data.LoadFromEnumerable(sessions);
        
        // Define trainer options.
        var options = new KMeansTrainer.Options
        {
            NumberOfClusters = 10,
            FeatureColumnName = "Embeddings"
        };

        // Define the trainer.
        var pipeline = mlContext.Clustering.Trainers.KMeans(options);
        // Train the model.
        var model = pipeline.Fit(trainingData);
        VBuffer<float>[]? centroids = default;
        model.Model.GetClusterCentroids(ref centroids, out var clustersCount);
        Console.WriteLine($"clusters count: {clustersCount}");
        
        var cleanCentroids = Enumerable.Range(0, centroids.Length).ToDictionary(x => x, x =>
        {
            var values = centroids[x].GetValues().ToArray();
            return values;
        });
        
        var predictor = mlContext.Model.CreatePredictionEngine<Session, ClusterPredictionModel>(model);

        foreach (var session in sessions)
        {
            var prediction = predictor.Predict(session);
            session.ClusterId = (int)prediction.ClusterId;
            session.Distances = prediction.Distances;
        }

        Console.WriteLine("Predicate completed");
        
        IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(chatDeployId, apiEndpoint, apiKey, loggerFactory: loggerFactory);
        
        foreach (var group in sessions.GroupBy(x=> x.ClusterId))
        {
            Console.WriteLine($"ClusterId: {group.Key}");

            if (cleanCentroids.TryGetValue(group.Key, out var centroid))
            {
                var sessionCollection = group.OrderByDescending(s=> 
                    TensorPrimitives.CosineSimilarity(centroid, s.Embeddings))
                    .ToArray();

                var prompt = $"""
                             You are a helpful assistant who is good at finding common in short words, 
                             please find out what the following session description have in common, exact common topics, and returns in one or two words directly,
                             `.NET`/`dotnet` should not be a valid words
                             for example: `Blazor`,`NuGet`, `MAUI` etc
                             ===== topics =====
                             {sessionCollection.Take(5).Select(x=> $"{x.Title}: {x.Description}").StringJoin($"{Environment.NewLine}==={Environment.NewLine}")}
                             """;
                var result = await chatCompletionService.GetChatMessageContentsAsync(prompt);
                var commonGround = result[^1].Content;
                Console.WriteLine($"Cluster: {group.Key}, {commonGround}");
                foreach (var session in sessionCollection)
                {
                    Console.WriteLine($"{session.Title}");
                }
            }

            Console.WriteLine();
        }
    }
}

file sealed class Session : SessionInputModel
{
    [NoColumn]
    public required DateTimeOffset BeginDateTime { get; init; }
    
    [NoColumn]
    public required DateTimeOffset EndDateTime { get; init; }
    
    [NoColumn]
    public required string Title { get; set; }
    
    [NoColumn]
    public required string Speaker { get; set; }
    
    [NoColumn]
    public required string Description { get; set; }

    [NoColumn]
    public int ClusterId { get; set; }
    [NoColumn]
    public float[] Distances { get; set; }
}

file class SessionInputModel
{
    public int SessionId { get; set; }
    
    [VectorType(1536)]
    public float[] Embeddings { get; set; }
}

public class ClusterPredictionModel
{
    [ColumnName("PredictedLabel")]
    public uint ClusterId { get; set; }

    [ColumnName("Score")]
    public float[] Distances { get; set; }
}
