using System.Collections;
using System.Runtime.CompilerServices;
using WeihanLi.Common.Helpers;

namespace CSharp13Samples;
internal class ParamsCollectionSample
{
    // https://github.com/dotnet/csharplang/issues/7700
    // https://github.com/dotnet/csharplang/blob/main/proposals/params-collections.md
    // https://github.com/dotnet/csharplang/blob/main/proposals/params-span.md
    public static void MainTest()
    {
        ParamsCollectionsUseSamples();
        ConsoleHelper.ReadLineWithPrompt();

        OverloadSamples();
        ConsoleHelper.ReadLineWithPrompt();

        Overload2Samples();
        ConsoleHelper.ReadLineWithPrompt();

        Overload3Samples();
        ConsoleHelper.ReadLineWithPrompt();

        Overload4Samples();
        ConsoleHelper.ReadLineWithPrompt();

        StringOverloadSamples();
    }

    public static void ParamsCollectionsUseSamples()
    {
        ParamsArrayMethod(1, 2, 3);
        ParamsListMethod(1, 2, 3);
        ParamsEnumerableMethod(1, 2, 3);

        ParamsSpanMethod(1, 2, 3);
        ParamsReadOnlySpanMethod(1, 2, 3);

        ParamsCustomCollectionMethod(1, 2, 3);
    }

    public static void OverloadSamples()
    {
        ParamsCollectionTest.OverloadTest(1, 2, 3);
        ParamsCollectionTest.OverloadTest([1, 2, 3]);
        ParamsCollectionTest.OverloadTest(new[] { 1, 2, 3 });
    }

    public static void Overload2Samples()
    {
        ParamsCollectionTest.OverloadTest2(1, 2, 3);
        ParamsCollectionTest.OverloadTest2([1, 2, 3]);
        ParamsCollectionTest.OverloadTest2(new[] { 1, 2, 3 });
        ParamsCollectionTest.OverloadTest2(Enumerable.Range(1, 3));
    }

    public static void Overload3Samples()
    {
        // CS0121: The call is ambiguous between the following methods or properties:
        //     'ParamsCollectionTest.OverloadTest3(params int[])' and 'ParamsCollectionTest.OverloadTest3(params List<int>)'
        ParamsCollectionTest.OverloadTest3(1, 2, 3);
        ParamsCollectionTest.OverloadTest3([1, 2, 3]);
        ParamsCollectionTest.OverloadTest3(new[] { 1, 2, 3 });
        ParamsCollectionTest.OverloadTest3(new List<int> { 1, 2, 3 });
        ParamsCollectionTest.OverloadTest3(Enumerable.Range(1, 3));
    }

    public static void Overload4Samples()
    {
        ParamsCollectionTest.OverloadTest4(1, 2, 3);
        ParamsCollectionTest.OverloadTest4([1, 2, 3]);
        ParamsCollectionTest.OverloadTest4(Enumerable.Range(1, 3));
    }

    public static void StringOverloadSamples()
    {
        ParamsCollectionTest.StringOverloadTest("Hello");
        ParamsCollectionTest.StringOverloadTest('H', 'e', 'l', 'l', 'o');
        ParamsCollectionTest.StringOverloadTest(['H', 'e', 'l', 'l', 'o']);
        ParamsCollectionTest.StringOverloadTest("Hello".AsSpan());
        ParamsCollectionTest.StringOverloadTest("Hello".ToCharArray());
    }


    private static void ParamsArrayMethod(params int[] array)
    {
        foreach (var item in array)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsReadOnlySpanMethod(params ReadOnlySpan<int> collection)
    {
        foreach (var item in collection)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsSpanMethod(params Span<int> collection)
    {
        foreach (var item in collection)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsListMethod(params List<int> list)
    {
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsEnumerableMethod(params IEnumerable<int> array)
    {
        foreach (var item in array)
        {
            Console.WriteLine(item);
        }
    }

    // custom collection expression
    private static void ParamsCustomCollectionMethod(params CustomNumberCollection collection)
    {
        foreach (var item in collection)
        {

            Console.WriteLine(item);
        }
    }
}


public class ParamsCollectionTest
{
    public int ParamsSpanMethod()
    {
        return ParamsOverloadMethod(1, 2, 3);
    }

    public int ParamsArrayMethod()
    {
        return ParamsOverloadMethod(new[] { 1, 2, 3 });
    }

    private int ParamsOverloadMethod(params ReadOnlySpan<int> span)
    {
        return span.Length;
    }

    private int ParamsOverloadMethod(params int[] array)
    {
        return array.Length;
    }

    public static void OverloadTest(params int[] array)
    {
        Console.WriteLine("Executing in Array method");
    }

    public static void OverloadTest(params ReadOnlySpan<int> span)
    {
        Console.WriteLine("Executing in Span method");
    }

    public static void OverloadTest2(params int[] array)
    {
        Console.WriteLine("Executing in Array method");
    }

    // [OverloadResolutionPriority(1)]
    public static void OverloadTest2(params IEnumerable<int> span)
    {
        Console.WriteLine("Executing in Span method");
    }

    public static void OverloadTest3(params IEnumerable<int> values)
    {
        Console.WriteLine("Executing in IEnumerable method");
    }

    [OverloadResolutionPriority(1)]
    public static void OverloadTest3(params int[] array)
    {
        Console.WriteLine("Executing in Array method");
    }

    public static void OverloadTest3(params List<int> values)
    {
        Console.WriteLine("Executing in List method");
    }

    public static void OverloadTest4(params IEnumerable<int> values)
    {
        Console.WriteLine("Executing in IEnumerable method");
    }

    public static void OverloadTest4(params ICollection<int> values)
    {
        Console.WriteLine("Executing in ICollection method");
    }

    public static void OverloadTest4(params IList<int> values)
    {
        Console.WriteLine("Executing in IList method");
    }

    public static void StringOverloadTest(string value)
    {
        Console.WriteLine("Executing in string method");
    }

    public static void StringOverloadTest(params char[] value)
    {
        Console.WriteLine("Executing in char method");
    }
    public static void StringOverloadTest(params ReadOnlySpan<char> value)
    {
        Console.WriteLine("Executing in span method");
    }
}

[CollectionBuilder(typeof(CustomCollectionBuilder), nameof(CustomCollectionBuilder.CreateNumber))]
internal sealed class CustomNumberCollection : IEnumerable<int>
{
    public required int[] Numbers { get; init; }
    public IEnumerator<int> GetEnumerator()
    {
        return Numbers.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)Numbers.GetEnumerator();
    }
}

file static class CustomCollectionBuilder
{
    public static CustomNumberCollection CreateNumber(ReadOnlySpan<int> elements)
    {
        return new CustomNumberCollection()
        {
            Numbers = elements.ToArray()
        };
    }
}
