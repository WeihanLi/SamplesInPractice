using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.TextToImage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace GetStarted;

public static class ArchStoryTeller
{
    public static async Task Run()
    {
        var apiEndpoint = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_ENDPOINT"));
        var deployment = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_CHAT_COMPLETION_DEPLOY_ID"));
        var apiKey = Guard.NotNullOrEmpty(EnvHelper.Val("AZURE_OPENAI_API_KEY"));
        var appId = EnvHelper.Val("WECHAT_APP_ID");
        var appSecret = EnvHelper.Val("WECHAT_APP_SECRET");
        var accessToken = EnvHelper.Val("WECHAT_ACCESS_TOKEN");
        var imagePath = EnvHelper.Val("WECHAT_TEST_IMAGE_PATH");

        using var httpClient = new HttpClient();

        if (string.IsNullOrEmpty(accessToken))
        {
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                Console.WriteLine("No appId or appSecret found");
            }
            else
            {
                // https://developers.weixin.qq.com/doc/offiaccount/Basic_Information/Get_access_token.html
                var getTokenUrl =
                    $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={appSecret}";
                var getTokenResponseText = await httpClient.GetStringAsync(getTokenUrl);
                var getTokenResponse = JsonSerializer.Deserialize<WechatAccessTokenModel>(getTokenResponseText);
                accessToken = getTokenResponse?.AccessToken;
                if (string.IsNullOrEmpty(accessToken))
                {
                    Console.WriteLine("Failed to get access token");
                }
            }
        }

        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deployment, apiEndpoint, apiKey)
            .AddAzureOpenAITextToImage("Dalle3", apiEndpoint, apiKey, "Dalle3")
            .Build();
        var topic = ConsoleHelper.ReadLineWithPrompt("Please provide a topic");
        if (string.IsNullOrEmpty(topic))
        {
            topic = "API Gateway";
        }

        Func<Task<Stream>> imageStreamFactory;
        string imageResponse;
        if (string.IsNullOrEmpty(imagePath))
        {
            // generate image according to the topic and outline
            var imagePrompt =
                $"Generate a cover image according to the topic  ```{topic}```, and the image is for technical developers, be technical and interesting";
            var textToImageService = kernel.GetRequiredService<ITextToImageService>();
            imageResponse = await textToImageService.GenerateImageAsync(imagePrompt, 1024, 1024);
            Console.WriteLine(imageResponse);
            imageStreamFactory = () => httpClient.GetStreamAsync(imageResponse);
        }
        else
        {
            imageResponse = imagePath;
            imageStreamFactory = () => File.OpenRead(imagePath).WrapTask<Stream>();
        }
        

        AddWechatMediaResponseModel? addMediaResponse = null;
        if (!string.IsNullOrEmpty(accessToken))
        {
            // imageStream for later usage
            var imageStream = new MemoryStream();
            await using var imageResponseStream = await imageStreamFactory().ConfigureAwait(false);
            await imageResponseStream.CopyToAsync(imageStream);
            imageStream.Seek(0, SeekOrigin.Begin);

            // https://developers.weixin.qq.com/doc/offiaccount/Asset_Management/Adding_Permanent_Assets.html
            var addMediaUrl =
                $"https://api.weixin.qq.com/cgi-bin/material/add_material?access_token={accessToken}&type=image";
            var imageFileName = $"{topic.Replace(' ', '_').ToLowerInvariant()}.png";

            var boundary = Guid.NewGuid().ToString("N");
            using var content = new MultipartFormDataContent(boundary);
            var fileContent = new StreamContent(imageStream);
            //Upload format reference:
            //https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1444738729
            //https://work.weixin.qq.com/api/doc#10112
            fileContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse($"form-data; name=\"media\"; filename=\"{imageFileName}\"; filelength={imageStream.Length}");
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            content.Add(fileContent);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");

            using var addMediaHttpResponse = await httpClient.PostAsync(addMediaUrl, content);
            var addMediaResponseText = await addMediaHttpResponse.Content.ReadAsStringAsync();
            addMediaHttpResponse.EnsureSuccessStatusCode();
            addMediaResponse = JsonSerializer.Deserialize<AddWechatMediaResponseModel>(addMediaResponseText);
            Console.WriteLine(addMediaResponse?.ToString());
            if (string.IsNullOrEmpty(addMediaResponse?.MediaId))
            {
                Console.WriteLine("Failed to add media");
                return;
            }
        }


        //
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        // draft story outline
        var outlinePrompt = $"""
                             Role:
                             You are an outstanding solution architect and story drafter, familiar with architectural design patterns and common solutions such as Kafka, flink, redis 
                             and other common product designs and common architectural patterns,
                             and are good at explaining the design and implementation of complex architectures and solutions in the form of short stories.

                             Task:
                             Draft a story outline according to the user input, and outline should be summarized in no more than 200 words.

                             User input is as follows:

                             {topic}
                             """;
        var outlineResponse = await chatCompletionService.GetChatMessageContentAsync(outlinePrompt,
            new AzureOpenAIPromptExecutionSettings() { MaxTokens = 300 });
        var storyOutline = outlineResponse.Content;
        Console.WriteLine(storyOutline);


        // draft story according to the outline
        var storyPrompt = $"""
                           Role:
                           You are an outstanding solution architect, familiar with architectural design patterns and common solutions such as Kafka, flink, redis 
                           and other common product designs and common architectural patterns, and are good at explaining the design 
                           and implementation of complex architectures and solutions in the form of short stories.

                           Task:
                           Craft a story according to the following outline, and daft the story by following the What,Why,How principal,
                           What's the background and problem of the story and why the {topic} is considered and how the {topic} solved the problem.
                           And response content in markdown directly.

                           Outline is as follows:

                           {outlineResponse}
                           """;
        var storyResponse = await chatCompletionService.GetChatMessageContentAsync(storyPrompt);
        var storyText = storyResponse.Content;
        Console.WriteLine(storyText);

        // translate
        var translatePrompt = $$"""
                                You're an excellent translator, please help translate Chinese to English or translate English to Chinese according to the user input and keep the markdown style.

                                The content need to be translated is as follows:

                                {{storyText}}
                                """;
        var translatedStoryResponse = await chatCompletionService.GetChatMessageContentAsync(translatePrompt);
        Console.WriteLine(translatedStoryResponse.Content);

        // replace image with uploaded image url
        if (!string.IsNullOrEmpty(addMediaResponse?.Url))
        {
            imageResponse = addMediaResponse.Url;
        }
        
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

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(addMediaResponse?.MediaId))
            return;

        // add draft
        // https://developers.weixin.qq.com/doc/offiaccount/Draft_Box/Add_draft.html
        var addDraftUrl = $"https://api.weixin.qq.com/cgi-bin/draft/add?access_token={accessToken}";
        var draftArticle = new
        {
            articles = new[]
            {
                new CreateWeChatArticleModel
                {
                    Title = topic, ThumbMediaId = addMediaResponse.MediaId, Content = englishStoryText
                },
                new CreateWeChatArticleModel
                {
                    Title = topic, ThumbMediaId = addMediaResponse.MediaId, Content = translatedStoryText
                }
            }
        };
        using var addDraftResponse = await httpClient.PostAsJsonAsync(addDraftUrl, draftArticle);
        addDraftResponse.EnsureSuccessStatusCode();
    }
}

file sealed record WechatAccessTokenModel
{
    [JsonPropertyName("access_token")] public required string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

file sealed record AddWechatMediaResponseModel
{
    [JsonPropertyName("media_id")] public required string MediaId { get; set; }

    [JsonPropertyName("url")] public required string Url { get; set; }
}

file sealed class CreateWeChatArticleModel
{
    [JsonPropertyName("title")] public required string Title { get; set; }

    [JsonPropertyName("thumb_media_id")] public required string ThumbMediaId { get; set; }

    [JsonPropertyName("content")] public required string Content { get; set; }

    [JsonPropertyName("author")] public string? Author { get; set; } = "WeihanLi";
}

public sealed class DingBotTextRequestModel
{
    [JsonPropertyName("msgtype")] public string MsgType => "text";

    [JsonPropertyName("text")] public required TextModel Text { get; set; }

    public sealed class TextModel
    {
        [JsonPropertyName("content")] public required string Content { get; set; }
    }
}
