using MathNet.Numerics;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using System.Text.Json;
using WeihanLi.Common;
using WeihanLi.Extensions;

namespace OpenAISample;

public static class EmbeddingSample
{
    
    public static async Task MainTest(IOpenAIService openAIService)
    {
        if (!File.Exists("./data/text-sample-with-embeddings.json"))
        {
            Console.WriteLine("embeddings not found, generate embeddings now");
            await GenerateEmbedding(openAIService);
        }
        
        await QuestionAnswerDemo(openAIService);
    }

    private static async Task GenerateEmbedding(IOpenAIService openAIService)
    {
        await using var fs = File.OpenRead("./data/text-sample.json");
        var dataList = await JsonSerializer.DeserializeAsync<SampleTextData[]>(fs, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        ArgumentNullException.ThrowIfNull(dataList);
        var success = false;
        foreach (var data in dataList)
        {
            var response = await openAIService.Embeddings.CreateEmbedding(new EmbeddingCreateRequest()
            {
                Input = data.Content.Replace("\r\n", " ").Replace("\n", " "), 
                Model = Models.TextEmbeddingAdaV2
            });
            if (response.Successful)
            {
                data.ContentVector = response.Data.FirstOrDefault()?.Embedding;
                Console.WriteLine(data.Content);
                // Console.WriteLine(data.ContentVector?.StringJoin(","));
                success = true;
            }
            else
            {
                Console.WriteLine("Error:");
                Console.WriteLine(response.Error.ToJson());
                break;
            }
        }
        
        if (!success) return;

        if (!Directory.Exists("./output"))
        {
            Directory.CreateDirectory("./output");
        }
        
        await using var wfs = File.Create("./output/text-sample-with-embeddings.json");
        await JsonSerializer.SerializeAsync(wfs, dataList);
        Console.WriteLine("Completed");
    }

    private static async Task QuestionAnswerDemo(IOpenAIService openAIService)
    {
        await Helpers.HandleInputLoopAsync(async input =>
        {
            var question = input.Replace("\r\n", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrEmpty(question))
            {
                return false;
            }

            var response = await openAIService.Embeddings.CreateEmbedding(new EmbeddingCreateRequest()
            {
                Input = question, Model = Models.TextEmbeddingAdaV2
            });
            if (response.Successful)
            {
                var questionVector = response.Data.FirstOrDefault()?.Embedding;
                ArgumentNullException.ThrowIfNull(questionVector);
                Console.WriteLine("Question embeddings generated");
                Console.WriteLine();
                var answer = await VectorSearchInMemory(questionVector, 1)
                    .ContinueWith(r => r.Result.FirstOrDefault());
                if (answer.IsNullOrEmpty())
                {
                    Console.WriteLine("No answer found");
                }
                else
                {
                    Console.WriteLine("Here's the possible answer found:");
                    Console.WriteLine(answer);
                }
                
                return true;
            }

            Console.WriteLine("Error:");
            Console.WriteLine(response.Error.ToJson());
            return false;
        });
    }

    private static async Task<string[]> VectorSearchInMemory(List<double> inputVector, int n)
    {
        await using var fs = File.OpenRead("./data/text-sample-with-embeddings.json");
        var dataList = await JsonSerializer.DeserializeAsync<SampleTextData[]>(fs);
        Guard.NotNull(dataList);
        var selectedAnswers = dataList.Where(x => x.ContentVector != null)
            .Select(x => new { x.Content, Distance = GetDistance(x.ContentVector!, inputVector) })
            .OrderBy(x => x.Distance)
            .Take(n)
            .Select(x=> x.Content)
            .ToArray();
        return selectedAnswers;
    }

    private static double GetDistance(List<double> p1, List<double> p2)
    {
        //
        // var sum = 0.0;
        // for (var i = 0; i < p1.Count; i++)
        // {
        //     var n = p1[i] - p2[i];
        //     sum += n * n;
        // }
        // return sum;
        
        return Distance.Cosine(p1.ToArray(), p2.ToArray());
    }
}

internal sealed class SampleTextData
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Category { get; set; }
    
    public List<double>? ContentVector { get; set; }
}
