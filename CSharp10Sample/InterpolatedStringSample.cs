using System.Text;

namespace CSharp10Sample;

public class InterpolatedStringSample
{
    public static void MainTest()
    {
        var num = 10;
        var str = $"Hello {num}";
        Console.WriteLine(str);
        Console.WriteLine();

        //
        FormattableString str1 = $"Hello {num}";
        Console.WriteLine(str1.Format);
        Console.WriteLine(str1.ToString());

        str1 = FormattableStringFactory.Create("Hello {0}", num);
        Console.WriteLine(str1.Format);
        Console.WriteLine(str1.ToString());

        //
        var stringHandler = new DefaultInterpolatedStringHandler();
        stringHandler.AppendLiteral("Hello ");
        stringHandler.AppendFormatted(num);
        var str2 = stringHandler.ToStringAndClear();
        Console.WriteLine(str2);

        // Custom InterpolatedStringHandler
        LogInterpolatedString("The num is 10");
        LogInterpolatedString($"The num is {num}");
    }

    private static void LogInterpolatedString(string str)
    {
        Console.WriteLine(nameof(LogInterpolatedString));
        Console.WriteLine(str);
    }

    private static void LogInterpolatedString(CustomInterpolatedStringHandler stringHandler)
    {
        Console.WriteLine(nameof(LogInterpolatedString));
        Console.WriteLine(nameof(CustomInterpolatedStringHandler));
        Console.WriteLine(stringHandler.ToString());
    }
}

// InterpolatedStringHandlerAttribute is required for custom InterpolatedStringHandler
[InterpolatedStringHandler]
public struct CustomInterpolatedStringHandler
{
    // Storage for the built-up string
    private readonly StringBuilder builder;

    /// <summary>
    /// CustomInterpolatedStringHandler constructor
    /// </summary>
    /// <param name="literalLength">string literal length</param>
    /// <param name="formattedCount">formatted count</param>
    public CustomInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        builder = new StringBuilder(literalLength);
        Console.WriteLine($"\tliteral length: {literalLength}, formattedCount: {formattedCount}");
    }

    // Required
    public void AppendLiteral(string s)
    {
        Console.WriteLine($"\tAppendLiteral called: {{{s}}}");
        builder.Append(s);
        Console.WriteLine($"\tAppended the literal string");
    }

    // Required
    public void AppendFormatted<T>(T t)
    {
        Console.WriteLine($"\tAppendFormatted called: {{{t}}} is of type {typeof(T)}");
        builder.Append(t?.ToString());
        Console.WriteLine($"\tAppended the formatted object");
    }

    public override string ToString()
    {
        return builder.ToString();
    }
}
