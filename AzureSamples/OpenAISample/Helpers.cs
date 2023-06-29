using WeihanLi.Extensions;

namespace OpenAISample;

public static class Helpers
{
    private static string? GetInput(string? prompt = null, bool insertNewLine = true)
    {
        Console.WriteLine();
        Console.WriteLine(prompt ?? "Enter your input here");
        var input = Console.ReadLine();
        if (insertNewLine)
        {
            Console.WriteLine();
        }
        return input;
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
