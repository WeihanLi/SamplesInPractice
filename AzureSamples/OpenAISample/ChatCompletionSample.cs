using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using WeihanLi.Extensions;

namespace OpenAISample;

public static class ChatCompletionSample
{
    public static async Task MainTest(IOpenAIService openAIService)
    {
        await Helpers.HandleInputLoopAsync(async input =>
        {
            input = input.Replace("\r\n", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            await GeneratePost(input, openAIService);
            return true;
        }, "Please input your topic to generate a blog post");
    }

    private static async Task GeneratePost(string prompt, IOpenAIService openAIService)
    {
        var response = await openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Model = Models.ChatGpt3_5Turbo,
            Messages = new List<ChatMessage>()
            {
                ChatMessage.FromSystem(
                    "You're an excellent content creator, you would help generate a post from a topic"),
                ChatMessage.FromUser($"Please generate a post with about 300 words from the topic: {prompt}")
            },
            Temperature = 1
        });
        if (response.Successful)
        {
            Console.WriteLine(response.Choices.First().Message.Content);
        }
        else
        {
            Console.WriteLine("Errored:");
            Console.WriteLine(response.Error.ToJson());
        }
    }
}
