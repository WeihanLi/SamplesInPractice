namespace CSharp13Samples;

// https://github.com/dotnet/csharplang/issues/140
// https://github.com/dotnet/csharplang/blob/main/proposals/field-keyword.md
internal static class FieldKeywordSample
{
    public static string Name
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
        //set;
    } = string.Empty;

    public static string Description
    {
        get => field ?? string.Empty;
        set
        {
            Console.Write(">>>");
            Console.WriteLine(field);
            Console.WriteLine($"Before set description, {nameof(value)}: {value}");
            field = value?.Trim()!;
            // CS0103: The name 'field' does not exist in the current context
            // Console.WriteLine($"After set description, {field}");
            Console.WriteLine("After set description");
            Console.Write(">>>");
            Console.WriteLine(field);
        }
    } = default!;

    public static void Run()
    {
        Name = " Hello ";
        Console.WriteLine(Name);
        Console.WriteLine(Name.Length);

        Console.WriteLine(Description is not null);
        Description = " World ";
        Console.WriteLine(Description);
        Console.WriteLine(Description.Length);
    }
}
