using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Text.Json;
using WeihanLi.Common;

namespace GetStarted;

public static class DotnetConfHelper
{
    public static async Task MainTest()
    {
        var deployId = Guard.NotNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_TEXT_EMBEDDING_DEPLOY_ID"));
        var apiEndpoint = Guard.NotNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_ENDPOINT"));
        var apiKey = Guard.NotNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));

        var textEmbeddingService = new AzureOpenAITextEmbeddingGenerationService(deployId, apiEndpoint, apiKey);
        
        var filePath = "dotnetconf2023-agenda.json";
        var filePathWithEmbeddings = "dotnetconf2023-agenda-with-embeddings.json";
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
                    var embeddings = await textEmbeddingService.GenerateEmbeddingsAsync(new[]
                    {
                        $"""
                        Speaker: {session.Speaker}
                        Title: {session.Title}
                        Description: {session.Description}
                        """
                    }).ContinueWith(r => r.Result[0]);
                    session.Embeddings = embeddings.ToArray();
                }
            }
            {
                await using var fs = File.Create(filePathWithEmbeddings);
                await JsonSerializer.SerializeAsync(fs, sessions);
            }
        }
        
        var mlContext = new MLContext(seed: 0);

        // Create a list of training data points.
        var trainingData = mlContext.Data.LoadFromEnumerable(sessions.Select(x => new SessionInputModel()
        {
            SessionId = x.SessionId,
            Embeddings = x.Embeddings.ToArray(),
        }));
        
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
        var predictor = mlContext.Model.CreatePredictionEngine<SessionInputModel, ClusterPredictionModel>(model);

        foreach (var session in sessions)
        {
            var prediction = predictor.Predict(session);
            session.ClusterId = prediction.ClusterId;
            session.Distances = prediction.Distances;
        }

        foreach (var group in sessions.GroupBy(x=> x.ClusterId))
        {
            Console.WriteLine(group.Key);
            foreach (var session in group)
            {
                Console.WriteLine(session.Speaker);
                Console.WriteLine(session.Title);
                Console.WriteLine(session.Description);
            }
        }
            
    }
}

file sealed class Session : SessionInputModel
{
    public required DateTimeOffset BeginDateTime { get; init; }
    public required DateTimeOffset EndDateTime { get; init; }
    public required string Title { get; set; }
    public required string Speaker { get; set; }
    public required string Description { get; set; }

    public int ClusterId { get; set; }
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
    public int ClusterId { get; set; }

    [ColumnName("Score")]
    public float[] Distances { get; set; }
}
