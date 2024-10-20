using System.Diagnostics.CodeAnalysis;

namespace CleanCSharpSamples;

public static class NameOfSamples
{
    private const string ArgmentName = nameof(ArgmentName);
    private const string ArgumentName = nameof(ArgumentName);
    private static readonly string AnotherArgumentName = nameof(AnotherArgumentName);
    
    public static void Run(string[] args)
    {
        Console.WriteLine("Executing in NameOfSamples...");
        Console.WriteLine($"Executing in {nameof(Run)}...");

        var title = "Engineer";
        Console.WriteLine($"{nameof(title)}: {title}");

        Console.WriteLine($"Arguments: {nameof(args)}-{string.Join(", ", args)}");
    }

    [return: NotNullIfNotNull(nameof(name))]
    private static string? MoreNameOfUsage([NotNullIfNotNull(nameof(description))]string? name, string? description)
    {
        if (!string.IsNullOrEmpty(description) && name is null)
        {
            throw new ArgumentException("The name cannot be null when description is not null or empty.");
        }
        return name;
    }
}
