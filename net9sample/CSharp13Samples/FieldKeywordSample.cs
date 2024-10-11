namespace CSharp13Samples;

internal static class FieldKeywordSample
{
    public static string Name
    { 
        get;
        // set => field = value?.Trim() ?? string.Empty;
        set;
    } = string.Empty;

    public static void MainTest()
    {
        Name = " Hello ";
        Console.WriteLine(Name);
        Console.WriteLine(Name.Length);
    }
}
