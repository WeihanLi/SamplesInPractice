using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSharp11Sample;
internal static class NameOfSample
{
    [Display(Name = nameof(MainTest))]
    public static void MainTest()
    {
        var displayName = MethodBase.GetCurrentMethod()
            ?.GetCustomAttribute<DisplayAttribute>()
            ?.Name;
        Console.WriteLine(displayName);

        Hello(1 + 1 > 2);
    }

    private static void Hello(bool condition, [CallerArgumentExpression(nameof(condition))] string? expression = null)
    {
        Console.WriteLine($"{expression} : {condition}");
    }
}
