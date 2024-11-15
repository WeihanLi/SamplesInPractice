using Markdig;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.TextToImage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Http;
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
                Console.WriteLine($"Get token response text: {getTokenResponseText}");
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
            Console.WriteLine($"addMediaResponseText: {addMediaResponseText}");
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
                           Craft a story according to the following outline, and daft the story by following the What,Why,How principal
                           What's the background and problem of the story and why this {topic} is considered and how it solved the problem.
                           And no less than 800 words, deep dive the {topic} and , response content in markdown directly without in a code block.

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

        // puppeteer render html

        // add draft
        await CreateDraft(addMediaResponse.MediaId, accessToken, topic, 
            englishStoryText, translatedStoryText, httpClient);
        Console.WriteLine("draft created");
    }

    private static Task<string> RenderMarkdown(string markdown)
    {
        // var browserFetcher = new BrowserFetcher();
        // await browserFetcher.DownloadAsync();
        // await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        // await using var page = await browser.NewPageAsync();
        // await page.GoToAsync("https://mdnice.weihanli.xyz", WaitUntilNavigation.Networkidle0);
        //
        // // 
        // return markdown;
        var html = Markdown.ToHtml(markdown);
        return Task.FromResult($"""<section data-tool="md-spark">{html}</section>""");
    }

    private static async Task CreateDraft(
        string mediaId, string accessToken, string title, 
        string content, string translatedContent,
        HttpClient httpClient
        )
    {
        // https://developers.weixin.qq.com/doc/offiaccount/Draft_Box/Add_draft.html
        var addDraftUrl = $"https://api.weixin.qq.com/cgi-bin/draft/add?access_token={accessToken}";
        var draftArticle = new
        {
            articles = new[]
            {
                new CreateWeChatArticleModel
                {
                    Title = title,
                    ThumbMediaId = mediaId,
                    Content = await RenderMarkdown(content)
                },
                new CreateWeChatArticleModel
                {
                    Title = title,
                    ThumbMediaId = mediaId,
                    Content = await RenderMarkdown(translatedContent)
                }
            }
        };
        var body = JsonSerializer.Serialize(draftArticle, JsonHelper.UnsafeEncoderOptions);
        using var requestBodyContent = new JsonHttpContent(body);
        using var addDraftResponse = await httpClient.PostAsync(addDraftUrl, requestBodyContent);
        var addDraftResponseText = await addDraftResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Add draft response: {addDraftResponse.StatusCode.ToString()} {addDraftResponseText}");
        addDraftResponse.EnsureSuccessStatusCode();
    }

    public static async Task CreateDraftTest()
    {
        var accessToken = EnvHelper.Val("WECHAT_ACCESS_TOKEN");
        if (string.IsNullOrEmpty(accessToken))
        {
            Console.WriteLine("Access token is missing");
            return;
        }
        
        var mediaId = "Sf8pqoLW74_bjK9ttqflCVDr95di9mX64CWLqMTfcrnMAuCJnRhAna4r_1fWUnnc";
        var content = """
                      ![](http://mmbiz.qpic.cn/mmbiz_png/wKg2Fib5uvkrjhNXGVrh1GR63NwLo8RicFOxMmC5vqvAd3D4CAfoCvoNmEyYdpetBSV6jOL4OlAegtJc2NSGvG0A/0?wx_fmt=png)
                      
                      # The Guardians of Data Harmony
                      
                      ## Setting
                      In a vibrant digital realm known as the "Data-verse," various factions battle for control over the realms of data integrity and reliability. Each faction is led by different algorithms, each harboring unique philosophies and methods of maintaining order amidst the tangled web of information.
                      
                      ## Characters
                      - **Raft:** A wise and noble leader advocating for simplicity and clarity in decision-making. Raft believes that with a clear structure, the chaos can be tamed and harmony restored.
                      - **Follower:** The loyal and committed allies who diligently uphold the decisions made by the community. They value collaboration and collective action.
                      - **Candidate:** The ambitious individuals who aspire to lead during times of crisis. These figures are often overzealous and set on securing their place at the helm.
                      - **Leader:** The chosen one who, once elected, is tasked with guiding the followers and ensuring their actions are synchronized.
                      
                      ## Plot
                      In the heart of the Data-verse, a perilous disturbance erupts as data inconsistencies wreak havoc across the realm. Factions engage in chaotic debates over the best strategies to establish a trustworthy system of governance that would strengthen their data integrity. As arguments grow louder and voices clash, the atmosphere becomes thick with confusion and distrust.
                      
                      Amidst the turmoil, Raft steps forward, a beacon of clarity in the chaos. With a calm demeanor, Raft introduces the concept of consensus through a systematic approach: elect a leader, communicate transparently, and replicate logs to maintain order. The democratic processes of leader elections become a lifeline for the Data-verse, suggesting that through collaboration and trust, they can overcome the disarray.
                      
                      ## Conflict
                      However, the path to harmony is fraught with tension. Several Candidates emerge, each vying for leadership and the chance to steer the community. Followers are torn between their enthusiasm for strong leadership and their deep-seated fears of being forgotten or sidelined in the process. Old rivalries flare as doubts arise concerning the trustworthiness of any potential leader, with whispers of betrayal shadowing their every move.
                      
                      ## Resolution
                      In this pivotal moment, Raft emphasizes the importance of collaboration and transparency. Through lively discussions and demonstrations of the leader election process, Raft illustrates how this method ensures resilience and stability within the community. The Candidates witness firsthand how a trusted Leader can synchronize the efforts of the followers, turning discord into a symphony of collaboration.
                      
                      Finally, the Data-verse unites, rallying behind Raft's vision. The collaborative spirit blossoms, and the factions solidify their commitment to data integrity through the principles of the Raft Consensus Algorithm. A new era dawns in the Data-verse—one where harmony is safeguarded, and every entity plays an integral role in the thriving ecosystem. Together, they embrace a future governed by trust, clarity, and the enduring wisdom of Raft.
                      
                      """;
        var translatedContent = """
                                ![](http://mmbiz.qpic.cn/mmbiz_png/wKg2Fib5uvkrjhNXGVrh1GR63NwLo8RicFOxMmC5vqvAd3D4CAfoCvoNmEyYdpetBSV6jOL4OlAegtJc2NSGvG0A/0?wx_fmt=png)
                                
                                # 数据和谐的守护者
                                
                                ## 背景
                                在一个充满活力的数字领域“数据宇宙”中，各个派系为了控制数据完整性和可靠性而展开斗争。每个派系都由不同的算法领导，这些算法各自拥有独特的哲学和在信息错综复杂的网络中维持秩序的方法。
                                
                                ## 角色
                                - **Raft（木筏）**：一位智慧而高尚的领袖，倡导在决策中追求简单明了。Raft相信，有了清晰的结构，混乱就能被驯服，和谐得以恢复。
                                - **追随者**：忠诚而坚定的盟友，他们认真维护社区作出的决策，重视合作与集体行动。
                                - **候选人**：在危机时刻渴望领导的雄心壮志的人物。这些人通常过于热情，决心要在领导岗位上占据一席之地。
                                - **领导者**：被选中的人，一旦当选，就负责引导追随者，确保他们的行动协调一致。
                                
                                ## 情节
                                在数据宇宙的核心，数据不一致引发了一场危险的骚乱，给整个领域带来了混乱。各个派系在建立可信的治理系统，以增强数据完整性方面进行激烈的辩论。随着争论声越来越高，声音交杂，空气中充满了困惑和不信任。
                                
                                在这场动荡中，Raft站了出来，成为混乱中的一束光。Raft以冷静的态度提出了共识的概念，通过一种系统化的方法：选举领导者，进行透明沟通，并复制日志以维持秩序。领导者选举的民主过程成为数据宇宙的重要救生索，表明通过合作与信任，他们可以克服混乱。
                                
                                ## 冲突
                                然而，通往和谐的道路充满了紧张情绪。几位候选人相继出现，各自争取领导权和指引社区的机会。追随者在对强有力领导的热情与深埋心中的被遗忘或被冷落的恐惧之间感到矛盾。当人们对任何潜在领导者的可信度产生怀疑，曾经的旧敌意再次浮现，背叛的低语在他们的每一个动作中潜行。
                                
                                ## 解决
                                在这一关键时刻，Raft强调了合作与透明的重要性。通过生动的讨论和选举过程的演示，Raft阐明了这一方法如何确保社区的韧性与稳定。候选人亲眼目睹了一个值得信赖的领导者如何能够协调追随者的努力，使纷争变为合作的交响乐。
                                
                                最终，数据宇宙团结起来，团结在Raft的愿景之下。合作精神蓬勃发展，各个派系通过Raft共识算法的原则巩固了对数据完整性的承诺。数据宇宙迎来了一个新纪元——一个和谐得到保护的时代，每个实体在繁荣的生态系统中发挥着不可或缺的作用。他们共同拥抱一个由信任、清晰和Raft持久智慧治理的未来。

                                """;
        
        var htmlContent = await RenderMarkdown(content);
        var translatedHtmlContent = await RenderMarkdown(translatedContent);
        
        using var httpClient = new HttpClient();
        await CreateDraft(mediaId, accessToken, "Raft Consensus Algorithm", htmlContent, translatedHtmlContent, httpClient);
    }
}

sealed record WechatAccessTokenModel
{
    [JsonPropertyName("access_token")] public required string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

sealed record AddWechatMediaResponseModel
{
    [JsonPropertyName("media_id")] public required string MediaId { get; set; }

    [JsonPropertyName("url")] public required string Url { get; set; }
}

sealed class CreateWeChatArticleModel
{
    [JsonPropertyName("title")] public required string Title { get; set; }

    [JsonPropertyName("thumb_media_id")] public required string ThumbMediaId { get; set; }

    [JsonPropertyName("content")] public required string Content { get; set; }

    [JsonPropertyName("author")] public string? Author { get; set; } = "WeihanLi";
}

sealed class DraftModel
{
    /// <summary>
    /// 图文消息的标题
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 图文消息的作者
    /// </summary>
    public string author { get; set; } = "WeihanLi";

    /// <summary>
    /// 图文消息的描述
    /// </summary>222-
    public string? digest { get; set; } = "";

    /// <summary>
    /// 图文消息页面的内容，支持HTML标签
    /// </summary>
    public string content { get; set; }

    /// <summary>
    /// 在图文消息页面点击“阅读原文”后的页面
    /// </summary>
    public string? content_source_url { get; set; }

    /// <summary>
    /// 图文消息缩略图的media_id，可以在基础支持上传多媒体文件接口中获得
    /// </summary>
    public string thumb_media_id { get; set; }

    /// <summary>
    /// 是否打开评论，0不打开，1打开
    /// </summary>
    public int need_open_comment { get; set; } = 1;
    /// <summary>
    /// 是否粉丝才可评论，0所有人可评论，1粉丝才可评论
    /// </summary>
    public int only_fans_can_comment { get; set; }
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
