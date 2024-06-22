using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections;
using System.Runtime.CompilerServices;
using WeihanLi.Common.Helpers;

namespace Net9Samples;

public static class CSharp13Samples
{
    // https://github.com/dotnet/csharplang/issues/7700
    // https://github.com/dotnet/csharplang/blob/main/proposals/params-collections.md
    // https://github.com/dotnet/csharplang/blob/main/proposals/params-span.md
    public static void ParamsCollectionSample()
    {
        {
            ParamsArrayMethod(1, 2, 3);
            ParamsListMethod(1, 2, 3);
            ParamsEnumerableMethod(1, 2, 3);

            ParamsSpanMethod(1, 2, 3);
            ParamsReadOnlySpanMethod(1, 2, 3);

            ParamsCustomCollectionMethod(1, 2, 3);
        }

        ConsoleHelper.ReadLineWithPrompt();

        {
            ParamsCollectionTest.OverloadTest(1, 2, 3);
            ParamsCollectionTest.OverloadTest([1, 2, 3]);
            ParamsCollectionTest.OverloadTest(new[] { 1, 2, 3 });

            ConsoleHelper.ReadLineWithPrompt();

            ParamsCollectionTest.OverloadTest2(1, 2, 3);
            ParamsCollectionTest.OverloadTest2([1, 2, 3]);
            ParamsCollectionTest.OverloadTest2(new[] { 1, 2, 3 });

            ConsoleHelper.ReadLineWithPrompt();

            ParamsCollectionTest.OverloadTest3(1, 2, 3);
            ParamsCollectionTest.OverloadTest3([1, 2, 3]);
            ParamsCollectionTest.OverloadTest3(Enumerable.Range(1, 3));

            ParamsCollectionTest.OverloadTest4(1, 2, 3);
            ParamsCollectionTest.OverloadTest4([1, 2, 3]);
            ParamsCollectionTest.OverloadTest4(Enumerable.Range(1, 3));
        }

        void ParamsReadOnlySpanMethod(params ReadOnlySpan<int> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsSpanMethod(params Span<int> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsListMethod(params List<int> list)
        {
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsEnumerableMethod(params IEnumerable<int> array)
        {
            foreach (var item in array)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsArrayMethod(params int[] array)
        {
            foreach (var item in array)
            {
                Console.WriteLine(item);
            }
        }

        void ParamsCustomCollectionMethod(params CustomNumberCollection collection)
        {
            foreach (var item in collection)
            {

                Console.WriteLine(item);
            }
        }
    }

    public static void ParamsSpanPerfTest()
    {
        BenchmarkRunner.Run<ParamsCollectionTest>();
    }

    //public class SemiFieldSample
    //{
    //    public string Name { get; set => field = value.Trim(); }
    //}

    // public static void ImplicitIndexAccess()
    // {
    //     int[] nums = 
    //     {
    //         [^1] = 1,
    //         [^2] = 2,
    //         [^3] = 3,
    //         [^4] = 4
    //     };
    //     Console.WriteLine(JsonSerializer.Serialize(nums));
    // }
}

[SimpleJob]
[MemoryDiagnoser]
public class ParamsCollectionTest
{
    [Benchmark(Baseline = true)]
    public int ParamsSpanMethod()
    {
        return ParamsOverloadMethod(1, 2, 3);
    }

    [Benchmark]
    public int ParamsArrayMethod()
    {
        return ParamsOverloadMethod(new[] { 1, 2, 3 });
    }

    private int ParamsOverloadMethod(params ReadOnlySpan<int> span)
    {
        // Debug.WriteLine("Executing in Span method");
        return 0;
    }

    private int ParamsOverloadMethod(params int[] array)
    {
        // Debug.WriteLine("Executing in Array method");
        return 0;
    }

    public static void OverloadTest(params int[] array)
    {
        Console.WriteLine("Executing in Array method");
    }

    public static void OverloadTest(params ReadOnlySpan<int> span)
    {
        Console.WriteLine("Executing in Span method");
    }

    // OverloadResolutionPriority not working so far(2024/06/20) https://github.com/dotnet/roslyn/issues/74067
    [OverloadResolutionPriority(1)]
    public static void OverloadTest2(params int[] array)
    {
        Console.WriteLine("Executing in Array method");
    }

    public static void OverloadTest2(params ReadOnlySpan<int> span)
    {
        Console.WriteLine("Executing in Span method");
    }

    public static void OverloadTest3(params IEnumerable<int> values)
    {
        Console.WriteLine("Executing in IEnumerable method");
    }

    public static void OverloadTest3(params int[] array)
    {
        Console.WriteLine("Executing in Array method");
    }

    //public static void OverloadTest3(params List<int> values)
    //{
    //    Console.WriteLine("Executing in List method");
    //}

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
}



[CollectionBuilder(typeof(CustomCollectionBuilder), nameof(CustomCollectionBuilder.CreateNumber))]
file sealed class CustomNumberCollection : IEnumerable<int>
{
    public required int[] Numbers { get; init; }
    public IEnumerator<int> GetEnumerator()
    {
        return Numbers.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Numbers.GetEnumerator();
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


// partial property, https://github.com/dotnet/csharplang/issues/6420
//public partial class PartialProperty
//{
//    public partial int Num { get; }
//}
//public partial class PartialProperty
//{
//    public partial int Num => 0;
//}
