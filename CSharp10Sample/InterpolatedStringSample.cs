using System.Globalization;
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
        Console.WriteLine(str1.ToString(new CultureInfo("zh-CN")));

        str1 = FormattableStringFactory.Create("Hello {0}", num);
        Console.WriteLine(str1.Format);
        Console.WriteLine(str1.ToString(new CultureInfo("en-US")));

        //
        var stringHandler = new DefaultInterpolatedStringHandler();
        stringHandler.AppendLiteral("Hello ");
        stringHandler.AppendFormatted(num);
        var str2 = stringHandler.ToStringAndClear();
        Console.WriteLine(str2);

        // Custom InterpolatedStringHandler
        LogInterpolatedString("The num is 10");
        Console.WriteLine();
        LogInterpolatedString($"The num is 10");
        Console.WriteLine();
        LogInterpolatedString($"The num is {num}");
        Console.WriteLine();
        // InterpolatedStringHandlerArgument
        LogInterpolatedString(10, $"The num is {num}");
        Console.WriteLine();
        LogInterpolatedString(15, $"The num is {num}");
    }

    private static void LogInterpolatedString(string str)
    {
        Console.WriteLine(nameof(LogInterpolatedString));
        Console.WriteLine(str);
    }

    private static void LogInterpolatedString(ref CustomInterpolatedStringHandler stringHandler)
    {
        Console.WriteLine(nameof(LogInterpolatedString));
        Console.WriteLine(nameof(CustomInterpolatedStringHandler));
        Console.WriteLine(stringHandler.ToString());
    }

    private static void LogInterpolatedString(int limit, [InterpolatedStringHandlerArgument("limit")] ref CustomInterpolatedStringHandler stringHandler)
    {
        Console.WriteLine(nameof(LogInterpolatedString));
        Console.WriteLine($"{nameof(CustomInterpolatedStringHandler)} with limit:{limit}");
        Console.WriteLine(stringHandler.ToString());
    }
}

// InterpolatedStringHandlerAttribute is required for custom InterpolatedStringHandler
[InterpolatedStringHandler]
public struct CustomInterpolatedStringHandler
{
    // Storage for the built-up string
    private readonly StringBuilder builder;

    private readonly int _limit;

    /// <summary>
    /// CustomInterpolatedStringHandler constructor
    /// </summary>
    /// <param name="literalLength">string literal length</param>
    /// <param name="formattedCount">formatted count</param>
    public CustomInterpolatedStringHandler(int literalLength, int formattedCount) : this(literalLength, formattedCount, 0)
    { }

    public CustomInterpolatedStringHandler(int literalLength, int formattedCount, int limit)
    {
        builder = new StringBuilder(literalLength);
        Console.WriteLine($"\tliteral length: {literalLength}, formattedCount: {formattedCount}");
        _limit = limit;
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
        if (t is int n && n < _limit)
        {
            return;
        }
        builder.Append(t?.ToString());
        Console.WriteLine($"\tAppended the formatted object");
    }

    public override string ToString()
    {
        return builder.ToString();
    }
}
