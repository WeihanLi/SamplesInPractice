﻿namespace CSharp13Samples;

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
            // still not working with .NET 9 GA SDK, working with latest vs preview 17.13.1 and vscode
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

file sealed class PropertySample
{
    public string? AutoName { get; set; }

    private string? _name;
    public string? Name
    {
        get => _name;
        set => _name = value;
    }

    public string? Name2
    {
        get => field;
        set => field = value;
    }

    public int Num { get => field > 0 ? field : 0; set; }
    public int Num2 { get; set => field = value > 0 ? value : 0; }
}
