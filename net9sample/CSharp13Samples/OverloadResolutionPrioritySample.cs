using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CSharp13Samples;
internal static class OverloadResolutionPrioritySample
{
    public static void MainTest()
    {
        int[] numbers = [1, 2, 3, 4];
        PrintNumbers(numbers);

        ReadOnlySpan<int> numbersSpan = numbers.AsSpan();
        PrintNumbers(numbersSpan);

        PrintNumbers(1, 2, 3);
        PrintNumbers([1, 2, 3]);

        PrintNumbers1(numbers);
        PrintNumbers1(numbersSpan);

        PrintNumbers1(1, 2, 3);
        PrintNumbers1([1, 2, 3]);

        Debug.Assert(numbers.Length != numbersSpan.Length);
    }


    private static void PrintNumbers(params int[] numbers)
    {
        Console.WriteLine("PrintNumbers in Array overload");
        foreach (var item in numbers)
        {
            Console.WriteLine(item);
        }
    }

    private static void PrintNumbers(params ReadOnlySpan<int> numbers)
    {
        Console.WriteLine("PrintNumbers in ReadOnlySpan overload");
        foreach (var item in numbers)
        {
            Console.WriteLine(item);
        }
    }

    [OverloadResolutionPriority(1)]
    private static void PrintNumbers1(params int[] numbers)
    {
        Console.WriteLine("PrintNumbers1 in Array overload");
        foreach (var item in numbers)
        {
            Console.WriteLine(item);
        }
    }

    private static void PrintNumbers1(params ReadOnlySpan<int> numbers)
    {
        Console.WriteLine("PrintNumbers1 in ReadOnlySpan overload");
        foreach (var item in numbers)
        {
            Console.WriteLine(item);
        }
    }
}
