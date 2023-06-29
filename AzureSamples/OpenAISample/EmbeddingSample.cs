using MathNet.Numerics;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using Polly;
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
        var input = Helpers.GetInput();
        while (input.IsNotNullOrEmpty() && input != "q")
        {
            var question = input.Replace("\r\n", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrEmpty(question))
            {
                break;
            }

            var response = await openAIService.Embeddings.CreateEmbedding(new EmbeddingCreateRequest()
            {
                Input = question, Model = Models.TextEmbeddingAdaV2
            });
            if (response.Successful)
            {
                var questionVector = response.Data.FirstOrDefault()?.Embedding;
                ArgumentNullException.ThrowIfNull(questionVector);
                var answers = await VectorSearchInMemory(questionVector, 3);
                if (answers.IsNullOrEmpty())
                {
                    Console.WriteLine("No answer found");
                }
                else
                {
                    var answerSelected = await GetSemanticAnswer(question, answers, openAIService);
                    if (string.IsNullOrEmpty(answerSelected) || "No answer found".Equals(answerSelected))
                    {
                        Console.WriteLine("No related answer found");
                    }
                    else
                    {
                        Console.WriteLine("Here's the answers found:");
                        Console.WriteLine(answerSelected);
                    }
                }

                input = Helpers.GetInput();
            }
            else
            {
                Console.WriteLine("Error:");
                Console.WriteLine(response.Error.ToJson());
                break;
            }
        }
        Console.WriteLine("Completed");
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
    
    private const string QuestionPromptFormat = """
                                    Please help choose one answer from the following candidates according to the question
                                    {0}
                                    If all the candidates do not make sense, please do not reply anything imagined, just say "No answer found"
                                    Answer candidates are as follows:
                                    {1}
                                    """;
    private static async Task<string> GetSemanticAnswer(string question, string[] answerCandidates, IOpenAIService openAIService)
    {
        var prompt = QuestionPromptFormat.FormatWith(question, answerCandidates.Select((a, i) => $"{i}: {a}"))
            .StringJoin(Environment.NewLine);
        
        var response = await openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Model = Models.ChatGpt3_5Turbo,
            Messages = new List<ChatMessage>()
            {
                ChatMessage.FromUser(prompt)
            },
            Temperature = 0
        });
        if (response.Successful)
        {
            var answer = response.Choices.First().Message.Content;
            // Console.WriteLine(answer);
            return answer;
        }
        else
        {
            Console.WriteLine("Errored:");
            Console.WriteLine(response.Error.ToJson());
            return string.Empty;
        }
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
