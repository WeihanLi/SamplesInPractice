using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace CleanCSharpSamples;

public static class CallerInfoSample
{
    private const string ArgmentName = nameof(ArgmentName);
    private const string ArgumentName = nameof(ArgumentName);
    
    public static void CallerMemberNameTest()
    {
        // ...
        LogSuccess();
    }
    
    public static void CallerMemberNameTest2()
    {
        // ...
        LogSuccess();
    }

    private static Counter<int>? _successCounter = null;
    private static void LogSuccess([CallerMemberName]string memberName = "")
    {
        // ...
        Console.WriteLine($"{memberName} invoked");
        _successCounter ??= new Meter("CleanCSharpSamples")
            .CreateCounter<int>("invoke_success_counter", "{times}", "Method invoke success count");
        _successCounter.Add(1, new KeyValuePair<string, object?>("method", memberName));
    }

    public static void CallerLocationTest()
    {
        try
        {
            var a = 0;
            // ReSharper disable once IntDivisionByZero
            Console.WriteLine(1/a);
            LogSuccess();
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }

    private static void LogException(Exception exception, 
        [CallerMemberName]string memberName = "", 
        [CallerFilePath]string filePath = "", 
        [CallerLineNumber]int lineNumber = 0
        )
    {
        Console.WriteLine($"{memberName} invoke exception, exception source:{filePath},{lineNumber}:\n{exception.Message}");
    }

    public static void CallerArgumentExpressionTest()
    {
        try
        {
            object? name = "test";
            ThrowIfNull(name);
        
            name = null;
            ThrowIfNull(name);
        }
        catch (ArgumentNullException e)
        {
            WriteLine(e.Message);
        }
        
        LogInputExpression(1 + 1);

        var num1 = 1;
        var num2 = 2;
        LogInputExpression(num1 + num2);
    }
    
    private static object ThrowIfNull(object? obj, [CallerArgumentExpression(nameof(obj))] string name = "")
    {
        if (obj is null)
            throw new ArgumentNullException(name);
        
        return obj;
    }

    private static void LogInputExpression(int input, [CallerArgumentExpression(nameof(input))] string name = "")
    {
        Console.WriteLine($"input expression: {name}, {nameof(input)}: {input}");
    }
}
