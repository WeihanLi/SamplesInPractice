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
}
