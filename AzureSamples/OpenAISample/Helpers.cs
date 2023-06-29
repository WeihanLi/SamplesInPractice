using WeihanLi.Extensions;

namespace OpenAISample;

public static class Helpers
{
    public static string? GetInput(string? prompt = null)
    {
        Console.WriteLine();
        Console.WriteLine(prompt ?? "Enter your input here");
        var input = Console.ReadLine();
        Console.WriteLine();
        return input;
    }

    public static void HandleInputLoop(Action<string> handler, string? inputPrompt = null, string exitInput = "q")
    {
        ArgumentNullException.ThrowIfNull(handler);
        var input = GetInput(inputPrompt);
        while (input.IsNotNullOrEmpty() && input != exitInput)
        {
            handler(input);

            input = GetInput(inputPrompt);
        }
    }

    public static async Task HandleInputLoopAsync(Func<string, Task<bool>> handler, string? inputPrompt = null, string exitInput = "q")
    {
        ArgumentNullException.ThrowIfNull(handler);
        var input = GetInput(inputPrompt);
        while (input.IsNotNullOrEmpty() && input != exitInput)
        {
            var result = await handler(input);
            if (!result) break;

            input = GetInput(inputPrompt);
        }
    }
}
