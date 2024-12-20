using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.TextToImage;
using PuppeteerSharp;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
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
            // imageStream 
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
                                """;
        var translatedStoryText = $"""
                                   ![]({imageResponse})

                                   {translatedStoryResponse.Content}
                                   """;

        var dingTalkNotificationUrl = EnvHelper.Val("DINGTALK_NOTIFICATION_URL");
        var dingBotSignature = "[amazingbot]";
        
        {
            using var enResponse = await httpClient.PostAsJsonAsync(dingTalkNotificationUrl,
                new DingBotTextRequestModel { Text = new()
                {
                    Content = $"{englishStoryText}\n{dingBotSignature}"
                } });
            enResponse.EnsureSuccessStatusCode();
        }
        {
            using var chineseResponse = await httpClient.PostAsJsonAsync(dingTalkNotificationUrl,
                new DingBotTextRequestModel { Text = new()
                {
                    Content = $"{translatedStoryText}\n{dingBotSignature}"
                } });
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

    private static readonly AsyncLock BrowserLock = new();
    private static IBrowser? _browser;
    
    private static async Task<string> RenderMarkdown(string markdown)
    {
        if (_browser is null)
        {
            using (await BrowserLock.LockAsync())
            {
                if (_browser is null)
                {
                    // https://github.com/hardkoded/puppeteer-sharp
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();
                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                }
            }
        }
        
        await using var page = await _browser.NewPageAsync();
        await page.GoToAsync("https://mdnice.weihanli.xyz", WaitUntilNavigation.Networkidle0);
        var html = await page.EvaluateFunctionAsync<string>("parseMdToHtml", markdown);
        await page.CloseAsync();
        return html;
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

        HttpResponseMessage? addDraftResponse = null;
        try
        {
            try
            {
                // bug
                addDraftResponse = await httpClient.PostAsJsonAsync(addDraftUrl, draftArticle,
                    JsonHelper.UnsafeEncoderOptions
                );
                addDraftResponse.EnsureSuccessStatusCode();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                addDraftResponse?.Dispose();

                var body = JsonSerializer.Serialize(draftArticle, JsonHelper.UnsafeEncoderOptions);
                using var requestBodyContent = new JsonHttpContent(body);
                addDraftResponse = await httpClient.PostAsync(addDraftUrl, requestBodyContent);
            }

            var addDraftResponseText = await addDraftResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Add draft response: {addDraftResponse.StatusCode.ToString()} {addDraftResponseText}");
            addDraftResponse.EnsureSuccessStatusCode();
        }
        finally
        {
            addDraftResponse?.Dispose();
        }
    }

    public static async Task CreateDraftTest()
    {
        var accessToken = EnvHelper.Val("WECHAT_ACCESS_TOKEN");
        // if (string.IsNullOrEmpty(accessToken))
        // {
        //     Console.WriteLine("Access token is missing");
        //     return;
        // }
        
        var mediaId = "Sf8pqoLW74_bjK9ttqflCVDr95di9mX64CWLqMTfcrnMAuCJnRhAna4r_1fWUnnc";
        
         var content = """
                      <section id="nice" data-tool="mdnice-editor" data-website="https://mdnice.weihanli.xyz" style="font-size: 16px; color: black; padding: 0 10px; line-height: 1.6; word-spacing: 0px; letter-spacing: 0px; word-break: break-word; word-wrap: break-word; text-align: left; font-family: Optima-Regular, Optima, PingFangSC-light, PingFangTC-light, 'PingFang SC', Cambria, Cochin, Georgia, Times, 'Times New Roman', serif;"><figure style="margin: 0; margin-top: 10px; margin-bottom: 10px; display: flex; flex-direction: column; justify-content: center; align-items: center;"><img src="http://mmbiz.qpic.cn/mmbiz_png/wKg2Fib5uvkrjhNXGVrh1GR63NwLo8RicFOxMmC5vqvAd3D4CAfoCvoNmEyYdpetBSV6jOL4OlAegtJc2NSGvG0A/0?wx_fmt=png" alt style="display: block; margin: 0 auto; max-width: 100%;"></figure>
                      <h1 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 24px;"><span class="prefix" style="display: none;"></span><span class="content">The Guardians of Data Harmony</span><span class="suffix"></span></h1>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Setting</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">In a vibrant digital realm known as the &quot;Data-verse,&quot; various factions battle for control over the realms of data integrity and reliability. Each faction is led by different algorithms, each harboring unique philosophies and methods of maintaining order amidst the tangled web of information.</p>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Characters</span><span class="suffix"></span></h2>
                      <ul style="margin-top: 8px; margin-bottom: 8px; padding-left: 25px; color: black; list-style-type: disc;">
                      <li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Raft:</strong> A wise and noble leader advocating for simplicity and clarity in decision-making. Raft believes that with a clear structure, the chaos can be tamed and harmony restored.</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Follower:</strong> The loyal and committed allies who diligently uphold the decisions made by the community. They value collaboration and collective action.</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Candidate:</strong> The ambitious individuals who aspire to lead during times of crisis. These figures are often overzealous and set on securing their place at the helm.</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Leader:</strong> The chosen one who, once elected, is tasked with guiding the followers and ensuring their actions are synchronized.</section></li></ul>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Plot</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">In the heart of the Data-verse, a perilous disturbance erupts as data inconsistencies wreak havoc across the realm. Factions engage in chaotic debates over the best strategies to establish a trustworthy system of governance that would strengthen their data integrity. As arguments grow louder and voices clash, the atmosphere becomes thick with confusion and distrust.</p>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">Amidst the turmoil, Raft steps forward, a beacon of clarity in the chaos. With a calm demeanor, Raft introduces the concept of consensus through a systematic approach: elect a leader, communicate transparently, and replicate logs to maintain order. The democratic processes of leader elections become a lifeline for the Data-verse, suggesting that through collaboration and trust, they can overcome the disarray.</p>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Conflict</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">However, the path to harmony is fraught with tension. Several Candidates emerge, each vying for leadership and the chance to steer the community. Followers are torn between their enthusiasm for strong leadership and their deep-seated fears of being forgotten or sidelined in the process. Old rivalries flare as doubts arise concerning the trustworthiness of any potential leader, with whispers of betrayal shadowing their every move.</p>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Resolution</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">In this pivotal moment, Raft emphasizes the importance of collaboration and transparency. Through lively discussions and demonstrations of the leader election process, Raft illustrates how this method ensures resilience and stability within the community. The Candidates witness firsthand how a trusted Leader can synchronize the efforts of the followers, turning discord into a symphony of collaboration.</p>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">Finally, the Data-verse unites, rallying behind Raft's vision. The collaborative spirit blossoms, and the factions solidify their commitment to data integrity through the principles of the Raft Consensus Algorithm. A new era dawns in the Data-verse—one where harmony is safeguarded, and every entity plays an integral role in the thriving ecosystem. Together, they embrace a future governed by trust, clarity, and the enduring wisdom of Raft.</p>
                      </section>
                      """;
        var content2 = """
                       <section id="nice" data-tool="mdnice-editor" data-website="https://mdnice.weihanli.xyz" style="font-size: 16px; color: black; padding: 0 10px; line-height: 1.6; word-spacing: 0px; letter-spacing: 0px; word-break: break-word; word-wrap: break-word; text-align: left; font-family: Optima-Regular, Optima, PingFangSC-light, PingFangTC-light, 'PingFang SC', Cambria, Cochin, Georgia, Times, 'Times New Roman', serif;"><figure style="margin: 0; margin-top: 10px; margin-bottom: 10px; display: flex; flex-direction: column; justify-content: center; align-items: center;"><img src="http://mmbiz.qpic.cn/mmbiz_png/wKg2Fib5uvkrjhNXGVrh1GR63NwLo8RicFOxMmC5vqvAd3D4CAfoCvoNmEyYdpetBSV6jOL4OlAegtJc2NSGvG0A/0?wx_fmt=png" alt style="display: block; margin: 0 auto; max-width: 100%;"></figure>
                       <h1 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 24px;"><span class="prefix" style="display: none;"></span><span class="content">数据和谐的守护者</span><span class="suffix"></span></h1>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">背景</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在一个充满活力的数字领域“数据宇宙”中，各个派系为了控制数据完整性和可靠性而展开斗争。每个派系都由不同的算法领导，这些算法各自拥有独特的哲学和在信息错综复杂的网络中维持秩序的方法。</p>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">角色</span><span class="suffix"></span></h2>
                       <ul style="margin-top: 8px; margin-bottom: 8px; padding-left: 25px; color: black; list-style-type: disc;">
                       <li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Raft（木筏）</strong>：一位智慧而高尚的领袖，倡导在决策中追求简单明了。Raft相信，有了清晰的结构，混乱就能被驯服，和谐得以恢复。</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">追随者</strong>：忠诚而坚定的盟友，他们认真维护社区作出的决策，重视合作与集体行动。</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">候选人</strong>：在危机时刻渴望领导的雄心壮志的人物。这些人通常过于热情，决心要在领导岗位上占据一席之地。</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">领导者</strong>：被选中的人，一旦当选，就负责引导追随者，确保他们的行动协调一致。</section></li></ul>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">情节</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在数据宇宙的核心，数据不一致引发了一场危险的骚乱，给整个领域带来了混乱。各个派系在建立可信的治理系统，以增强数据完整性方面进行激烈的辩论。随着争论声越来越高，声音交杂，空气中充满了困惑和不信任。</p>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在这场动荡中，Raft站了出来，成为混乱中的一束光。Raft以冷静的态度提出了共识的概念，通过一种系统化的方法：选举领导者，进行透明沟通，并复制日志以维持秩序。领导者选举的民主过程成为数据宇宙的重要救生索，表明通过合作与信任，他们可以克服混乱。</p>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">冲突</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">然而，通往和谐的道路充满了紧张情绪。几位候选人相继出现，各自争取领导权和指引社区的机会。追随者在对强有力领导的热情与深埋心中的被遗忘或被冷落的恐惧之间感到矛盾。当人们对任何潜在领导者的可信度产生怀疑，曾经的旧敌意再次浮现，背叛的低语在他们的每一个动作中潜行。</p>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">解决</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在这一关键时刻，Raft强调了合作与透明的重要性。通过生动的讨论和选举过程的演示，Raft阐明了这一方法如何确保社区的韧性与稳定。候选人亲眼目睹了一个值得信赖的领导者如何能够协调追随者的努力，使纷争变为合作的交响乐。</p>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">最终，数据宇宙团结起来，团结在Raft的愿景之下。合作精神蓬勃发展，各个派系通过Raft共识算法的原则巩固了对数据完整性的承诺。数据宇宙迎来了一个新纪元——一个和谐得到保护的时代，每个实体在繁荣的生态系统中发挥着不可或缺的作用。他们共同拥抱一个由信任、清晰和Raft持久智慧治理的未来。</p>
                       </section>
                       """;

        using var httpClientHandler = new HttpClientHandler();
        using var httpLoggingHandler = new HttpLoggingHandler(httpClientHandler);
        using var httpClient = new HttpClient(httpClientHandler);
        await CreateDraft(mediaId, accessToken ?? "", "Raft Consensus Algorithm", content, content2, httpClient);
    }

    public static async Task JsonContentTest()
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web).WithUnsafeEncoder();

        var content = """
                      <section id="nice" data-tool="mdnice-editor" data-website="https://mdnice.weihanli.xyz" style="font-size: 16px; color: black; padding: 0 10px; line-height: 1.6; word-spacing: 0px; letter-spacing: 0px; word-break: break-word; word-wrap: break-word; text-align: left; font-family: Optima-Regular, Optima, PingFangSC-light, PingFangTC-light, 'PingFang SC', Cambria, Cochin, Georgia, Times, 'Times New Roman', serif;"><figure style="margin: 0; margin-top: 10px; margin-bottom: 10px; display: flex; flex-direction: column; justify-content: center; align-items: center;"><img src="http://mmbiz.qpic.cn/mmbiz_png/wKg2Fib5uvkrjhNXGVrh1GR63NwLo8RicFOxMmC5vqvAd3D4CAfoCvoNmEyYdpetBSV6jOL4OlAegtJc2NSGvG0A/0?wx_fmt=png" alt style="display: block; margin: 0 auto; max-width: 100%;"></figure>
                      <h1 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 24px;"><span class="prefix" style="display: none;"></span><span class="content">The Guardians of Data Harmony</span><span class="suffix"></span></h1>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Setting</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">In a vibrant digital realm known as the &quot;Data-verse,&quot; various factions battle for control over the realms of data integrity and reliability. Each faction is led by different algorithms, each harboring unique philosophies and methods of maintaining order amidst the tangled web of information.</p>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Characters</span><span class="suffix"></span></h2>
                      <ul style="margin-top: 8px; margin-bottom: 8px; padding-left: 25px; color: black; list-style-type: disc;">
                      <li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Raft:</strong> A wise and noble leader advocating for simplicity and clarity in decision-making. Raft believes that with a clear structure, the chaos can be tamed and harmony restored.</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Follower:</strong> The loyal and committed allies who diligently uphold the decisions made by the community. They value collaboration and collective action.</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Candidate:</strong> The ambitious individuals who aspire to lead during times of crisis. These figures are often overzealous and set on securing their place at the helm.</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Leader:</strong> The chosen one who, once elected, is tasked with guiding the followers and ensuring their actions are synchronized.</section></li></ul>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Plot</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">In the heart of the Data-verse, a perilous disturbance erupts as data inconsistencies wreak havoc across the realm. Factions engage in chaotic debates over the best strategies to establish a trustworthy system of governance that would strengthen their data integrity. As arguments grow louder and voices clash, the atmosphere becomes thick with confusion and distrust.</p>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">Amidst the turmoil, Raft steps forward, a beacon of clarity in the chaos. With a calm demeanor, Raft introduces the concept of consensus through a systematic approach: elect a leader, communicate transparently, and replicate logs to maintain order. The democratic processes of leader elections become a lifeline for the Data-verse, suggesting that through collaboration and trust, they can overcome the disarray.</p>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Conflict</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">However, the path to harmony is fraught with tension. Several Candidates emerge, each vying for leadership and the chance to steer the community. Followers are torn between their enthusiasm for strong leadership and their deep-seated fears of being forgotten or sidelined in the process. Old rivalries flare as doubts arise concerning the trustworthiness of any potential leader, with whispers of betrayal shadowing their every move.</p>
                      <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">Resolution</span><span class="suffix"></span></h2>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">In this pivotal moment, Raft emphasizes the importance of collaboration and transparency. Through lively discussions and demonstrations of the leader election process, Raft illustrates how this method ensures resilience and stability within the community. The Candidates witness firsthand how a trusted Leader can synchronize the efforts of the followers, turning discord into a symphony of collaboration.</p>
                      <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">Finally, the Data-verse unites, rallying behind Raft's vision. The collaborative spirit blossoms, and the factions solidify their commitment to data integrity through the principles of the Raft Consensus Algorithm. A new era dawns in the Data-verse—one where harmony is safeguarded, and every entity plays an integral role in the thriving ecosystem. Together, they embrace a future governed by trust, clarity, and the enduring wisdom of Raft.</p>
                      </section>
                      """;
        var content2 = """
                       <section id="nice" data-tool="mdnice-editor" data-website="https://mdnice.weihanli.xyz" style="font-size: 16px; color: black; padding: 0 10px; line-height: 1.6; word-spacing: 0px; letter-spacing: 0px; word-break: break-word; word-wrap: break-word; text-align: left; font-family: Optima-Regular, Optima, PingFangSC-light, PingFangTC-light, 'PingFang SC', Cambria, Cochin, Georgia, Times, 'Times New Roman', serif;"><figure style="margin: 0; margin-top: 10px; margin-bottom: 10px; display: flex; flex-direction: column; justify-content: center; align-items: center;"><img src="http://mmbiz.qpic.cn/mmbiz_png/wKg2Fib5uvkrjhNXGVrh1GR63NwLo8RicFOxMmC5vqvAd3D4CAfoCvoNmEyYdpetBSV6jOL4OlAegtJc2NSGvG0A/0?wx_fmt=png" alt style="display: block; margin: 0 auto; max-width: 100%;"></figure>
                       <h1 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 24px;"><span class="prefix" style="display: none;"></span><span class="content">数据和谐的守护者</span><span class="suffix"></span></h1>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">背景</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在一个充满活力的数字领域“数据宇宙”中，各个派系为了控制数据完整性和可靠性而展开斗争。每个派系都由不同的算法领导，这些算法各自拥有独特的哲学和在信息错综复杂的网络中维持秩序的方法。</p>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">角色</span><span class="suffix"></span></h2>
                       <ul style="margin-top: 8px; margin-bottom: 8px; padding-left: 25px; color: black; list-style-type: disc;">
                       <li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">Raft（木筏）</strong>：一位智慧而高尚的领袖，倡导在决策中追求简单明了。Raft相信，有了清晰的结构，混乱就能被驯服，和谐得以恢复。</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">追随者</strong>：忠诚而坚定的盟友，他们认真维护社区作出的决策，重视合作与集体行动。</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">候选人</strong>：在危机时刻渴望领导的雄心壮志的人物。这些人通常过于热情，决心要在领导岗位上占据一席之地。</section></li><li><section style="margin-top: 5px; margin-bottom: 5px; line-height: 26px; text-align: left; color: rgb(1,1,1); font-weight: 500;"><strong style="font-weight: bold; color: black;">领导者</strong>：被选中的人，一旦当选，就负责引导追随者，确保他们的行动协调一致。</section></li></ul>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">情节</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在数据宇宙的核心，数据不一致引发了一场危险的骚乱，给整个领域带来了混乱。各个派系在建立可信的治理系统，以增强数据完整性方面进行激烈的辩论。随着争论声越来越高，声音交杂，空气中充满了困惑和不信任。</p>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在这场动荡中，Raft站了出来，成为混乱中的一束光。Raft以冷静的态度提出了共识的概念，通过一种系统化的方法：选举领导者，进行透明沟通，并复制日志以维持秩序。领导者选举的民主过程成为数据宇宙的重要救生索，表明通过合作与信任，他们可以克服混乱。</p>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">冲突</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">然而，通往和谐的道路充满了紧张情绪。几位候选人相继出现，各自争取领导权和指引社区的机会。追随者在对强有力领导的热情与深埋心中的被遗忘或被冷落的恐惧之间感到矛盾。当人们对任何潜在领导者的可信度产生怀疑，曾经的旧敌意再次浮现，背叛的低语在他们的每一个动作中潜行。</p>
                       <h2 style="margin-top: 30px; margin-bottom: 15px; padding: 0px; font-weight: bold; color: black; font-size: 22px;"><span class="prefix" style="display: none;"></span><span class="content">解决</span><span class="suffix"></span></h2>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">在这一关键时刻，Raft强调了合作与透明的重要性。通过生动的讨论和选举过程的演示，Raft阐明了这一方法如何确保社区的韧性与稳定。候选人亲眼目睹了一个值得信赖的领导者如何能够协调追随者的努力，使纷争变为合作的交响乐。</p>
                       <p style="font-size: 16px; padding-top: 8px; padding-bottom: 8px; margin: 0; line-height: 26px; color: black;">最终，数据宇宙团结起来，团结在Raft的愿景之下。合作精神蓬勃发展，各个派系通过Raft共识算法的原则巩固了对数据完整性的承诺。数据宇宙迎来了一个新纪元——一个和谐得到保护的时代，每个实体在繁荣的生态系统中发挥着不可或缺的作用。他们共同拥抱一个由信任、清晰和Raft持久智慧治理的未来。</p>
                       </section>
                       """;
        var draft = new
        {
            articles = new[]
            {
                new CreateWeChatArticleModel
                {
                    Title = "test",
                    ThumbMediaId = "",
                    Content = content
                },
                new CreateWeChatArticleModel
                {
                    Title = "test",
                    ThumbMediaId = "",
                    Content = content2
                }
            }
        };
        using var jsonContent = JsonContent.Create(draft, options: jsonOptions);
        var jsonContentText = await jsonContent.ReadAsStringAsync();

        IsValidJson(jsonContentText);
        Console.WriteLine();
        var serializedContentText = JsonSerializer.Serialize(draft, jsonOptions);
        Console.WriteLine($"Content Equals: {jsonContentText == serializedContentText}");

        // var url = "https://api.weixin.qq.com/cgi-bin/draft/add";
        var url = "https://test.weihanli.xyz/test";
        using var httpHandler = new HttpLoggingHandler();
        using var httpClient = new HttpClient(httpHandler);
        using var response1 = await httpClient.PostAsJsonAsync(url, draft, jsonOptions);
        Console.WriteLine($"Response StatusCode: {response1.StatusCode}, Content: {await response1.Content.ReadAsStringAsync()}");
        using var response2 = await httpClient.PostAsync(url, jsonContent);
        Console.WriteLine($"Response StatusCode: {response2.StatusCode}, Content: {await response2.Content.ReadAsStringAsync()}");
    }

    internal static void IsValidJson(string jsonContent)
    {
        try
        {
            _ = JsonNode.Parse(jsonContent);
            Console.WriteLine("Valid Json Content");
        }
        catch
        {
            Console.WriteLine($"Invalid Json Content: {jsonContent}");
        }
    }
}

file sealed class HttpLoggingHandler : DelegatingHandler
{
    public HttpLoggingHandler() : this(new HttpClientHandler())
    {
    }
    
    public HttpLoggingHandler(HttpMessageHandler innerHandler)
    {
        InnerHandler = innerHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if(request.Content != null)
        {
            var requestHeaders = request.Content.Headers.AsEnumerable().Concat(request.Headers)
                .Select(x=> new { x.Key, x.Value })
                .ToArray();
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Headers: {requestHeaders.ToJson()}, requestBody: {requestBody}");
            ArchStoryTeller.IsValidJson(requestBody);
        }

        return await base.SendAsync(request, cancellationToken);
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
    
    [JsonPropertyName("need_open_comment")]
    public int EnableComment { get; set; } = 1;
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
