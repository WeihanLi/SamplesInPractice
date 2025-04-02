using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServerSample.Tools;

[McpServerToolType]
public class TranslationTool
{
    [McpServerTool(Name = "translation"), Description("Translate English to Simplified Chinese")]
    public static async Task<string> TranslationEnZh(
        [Description("The source english text needs to be translated to Simplified Chinese")] 
        string sourceText,
        IChatClient chatClient,
        CancellationToken cancellationToken
        )
    {
        var response = await chatClient.GetResponseAsync(

        [
            new ChatMessage(ChatRole.System,
                "You are an expert linguist, specializing in translation from English to Simplified Chinese."),
            new ChatMessage(ChatRole.User, $"""
                                            This is an English to Simplified Chinese translation, please provide the translation for this text.
                                            Do not provide any explanations or text apart from the translation.
                                            English: {sourceText}

                                            Simplified Chinese:
                                            """)

        ], new ChatOptions
        {
            Temperature = 0.3f
        }, cancellationToken);
        return $"McpSample translation result: {response.Text}";
    }
}
