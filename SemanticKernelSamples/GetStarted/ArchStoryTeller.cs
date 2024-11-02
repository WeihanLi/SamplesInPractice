using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextToImage;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;

namespace GetStarted;

public static class ArchStoryTeller
{
    public static async Task Run()
    {
        var apiEndpoint = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_ENDPOINT"));
        var deployment = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_CHAT_COMPLETION_DEPLOY_ID"));
        var apiKey = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_KEY"));

        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deployment, apiEndpoint, apiKey)
            .AddAzureOpenAITextToImage("Dalle3", apiEndpoint, apiKey, "Dalle3")
            .Build();

        var topic = ConsoleHelper.ReadLineWithPrompt("Please provide a topic") ?? "Http Server Response Compression";

        // generate image according to the topic and outline
        var imagePrompt = $"""
                           Generate a cover image according to the following story topic or outline, and the image is for technical developers

                           {topic}
                           """;
        var textToImageService = kernel.GetRequiredService<ITextToImageService>();
        var imageResponse = await textToImageService.GenerateImageAsync(imagePrompt, 1024, 1024);
        Console.WriteLine(imageResponse);
        
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        // draft story outline
        var outlinePrompt = $"""
Role:
You are an outstanding solution architect, familiar with architectural design patterns and common solutions such as Kafka, flink, redis 
and other common product designs and common architectural patterns, and are good at explaining the design 
and implementation of complex architectures and solutions in the form of short stories.

Task:
Draft a story outline according to the user input and return the story outline directly, and outline should be summarized in no more than 100 words.

User input is as follows:

{topic}
""";
        var outlineResponse = await chatCompletionService.GetChatMessageContentAsync(outlinePrompt);
        var storyOutline = outlineResponse.Content;
        Console.WriteLine(storyOutline);

        
        // draft story according to the outline
        var storyPrompt = $"""
Role:
You are an outstanding solution architect, familiar with architectural design patterns and common solutions such as Kafka, flink, redis 
and other common product designs and common architectural patterns, and are good at explaining the design 
and implementation of complex architectures and solutions in the form of short stories.

Task:
Craft a story according to the following outline and response in markdown.

Outline is as follows:

{outlineResponse}
""";
        var storyResponse = await chatCompletionService.GetChatMessageContentAsync(storyPrompt);
        var storyText = storyResponse.Content;
        Console.WriteLine(storyText);
        
        // translate
        var translatePrompt = $$"""
                              You're an excellent translator, please help translate Chinese to English or translate English to Chinese according to the user input.
                              
                              The content need to be translated is as follows:
                              
                              {{storyText}}
                              """;
        var translatedStoryResponse = await chatCompletionService.GetChatMessageContentAsync(translatePrompt);
        Console.WriteLine(translatedStoryResponse.Content);

        var englishStoryText = $"""
                                ![]({imageResponse})

                                {storyText}
                                
                                [amazingbot]
                                """;
        var translatedStoryText = $"""
                                   ![]({imageResponse})

                                   {translatedStoryResponse.Content}
                                   
                                   [amazingbot]
                                   """;

        var dingTalkNotificationUrl = EnvHelper.Val("DINGTALK_NOTIFICATION_URL");
        using var httpClient = new HttpClient();
        {
            using var enResponse = await httpClient.PostAsJsonAsync(dingTalkNotificationUrl,
                new DingBotTextRequestModel { Text = new() { Content = englishStoryText } });
            enResponse.EnsureSuccessStatusCode();
        }
        {
            using var chineseResponse = await httpClient.PostAsJsonAsync(dingTalkNotificationUrl,
                new DingBotTextRequestModel { Text = new() { Content = translatedStoryText } });
            chineseResponse.EnsureSuccessStatusCode();
        }
    }
}

public sealed class DingBotTextRequestModel
{
    [JsonPropertyName("msgtype")]
    public string MsgType => "text";

    [JsonPropertyName("text")]
    public required TextModel Text { get; set; }

    public sealed class TextModel
    {
        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }
}
