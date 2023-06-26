using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using WeihanLi.Extensions;

namespace OpenAISample;

public static class ChatCompletionSample
{
    public static async Task MainTest(IOpenAIService openAIService)
    {
        var input = Helpers.GetInput();
        while (input.IsNotNullOrEmpty() && input != "q")
        {
            input = input.Replace("\r\n", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrEmpty(input))
            {
                break;
            }

            await GeneratePost(input, openAIService);

            input = Helpers.GetInput();
        }
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
            }
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
