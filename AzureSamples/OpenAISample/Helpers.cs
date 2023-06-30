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

    // https://github.com/accord-net/framework/blob/49f2932a7baf69c7f6113ec82976aecca0c50353/Sources/Accord.Math/Distances/Cosine.cs#L92
    public static double GetCosineSimilarity(IList<double> x, IList<double> y)
    {
        double sum = 0;
        double p = 0;
        double q = 0;

        for (var i = 0; i < x.Count; i++)
        {
            sum += x[i] * y[i];
            p += x[i] * x[i];
            q += y[i] * y[i];
        }

        var den = Math.Sqrt(p) * Math.Sqrt(q);
        return (sum == 0 || den == 0) ? 0 : sum / den;
    }
}
