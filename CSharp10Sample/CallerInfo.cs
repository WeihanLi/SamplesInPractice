namespace CSharp10Sample;

public class CallerInfo
{
    public static void MainTest()
    {
        // 我是谁，我在哪儿
        DumpCallerInfo();

        object? xiaoMing = null;
        InvokeHelper.TryInvoke(() => ArgumentNullException.ThrowIfNull(xiaoMing));

        Console.ReadLine();

        var action = () => { };
        InvokeHelper.TryInvoke(() => ValidateArgument(action, action is null));

        InvokeHelper.TryInvoke(() => Verify.NotNull((object?)null));

        var num = 10;
        InvokeHelper.TryInvoke(() => Verify.InRange(num, 2, 5));
        InvokeHelper.TryInvoke(() => Verify.InRange(num, 10 + 2, 10 + 5));

        InvokeHelper.TryInvoke(() => Verify.Argument(action is null, "Bad params"));

        var name = string.Empty;
        InvokeHelper.TryInvoke(() => Verify.NotNullOrEmpty(name));
    }

    private static void ValidateArgument(object parameter, bool condition, [CallerArgumentExpression("parameter")] string? parameterName = null, [CallerArgumentExpression("condition")] string? message = null)
    {
        if (parameter is null)
        {
            ArgumentNullException.ThrowIfNull(parameter);
        }
        if (!condition)
        {
            throw new ArgumentException($"Argument failed validation: <{message}>", parameterName);
        }
    }

    private static void DumpCallerInfo(
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null
    )
    {
        Console.WriteLine("Caller info:");
        Console.WriteLine($@"CallerFilePath: {callerFilePath}
CallerLineNumber: {callerLineNumber}
CallerMemberName: {callerMemberName}
");
    }
}

public static class Verify
{
    public static void Argument(bool condition, string message, [CallerArgumentExpression("condition")] string? conditionExpression = null)
    {
        if (!condition) throw new ArgumentException(message: message, paramName: conditionExpression);
    }

    public static void NotNullOrEmpty(string argument, [CallerArgumentExpression("argument")] string? argumentExpression = null)
    {
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException("Can not be null or empty", argumentExpression);
        }
    }

    public static void InRange(int argument, int low, int high,
        [CallerArgumentExpression("argument")] string? argumentExpression = null,
        [CallerArgumentExpression("low")] string? lowExpression = null,
        [CallerArgumentExpression("high")] string? highExpression = null)
    {
        if (argument < low)
        {
            throw new ArgumentOutOfRangeException(paramName: argumentExpression,
                message: $"{argumentExpression} ({argument}) cannot be less than {lowExpression} ({low}).");
        }

        if (argument > high)
        {
            throw new ArgumentOutOfRangeException(paramName: argumentExpression,
                message: $"{argumentExpression} ({argument}) cannot be greater than {highExpression} ({high}).");
        }
    }

    public static void NotNull<T>(T? argument, [CallerArgumentExpression("argument")] string? argumentExpression = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(argument, argumentExpression);
    }
}
