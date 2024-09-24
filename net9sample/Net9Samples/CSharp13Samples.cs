using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Net9Samples;

public static class CSharp13Samples
{
    public static void ParamsSpanPerfTest()
    {
        BenchmarkRunner.Run<ParamsCollectionTest>();
    }

    public static void RefStructPerfTest()
    {
        BenchmarkRunner.Run<RefStructInterfaceBenchmark>();
    }
}

[SimpleJob]
[MemoryDiagnoser]
public class ParamsCollectionTest
{
    [Benchmark(Baseline = true)]
    public int ParamsSpanMethod()
    {
        var num = 0;
        for (var i = 0; i < 10_000; i++)
        {
            num += ParamsOverloadSpanMethod(1, 2, 3);
        }
        return num;
    }

    [Benchmark]
    public int ParamsArrayMethod()
    {
        var num = 0;
        for (var i = 0; i < 10_000; i++)
        {
            num += ParamsOverloadArrayMethod(1, 2, 3);
        }
        return num;
    }

    private int ParamsOverloadSpanMethod(params ReadOnlySpan<int> span) => span.Length;

    private int ParamsOverloadArrayMethod(params int[] array) => array.Length;
}

[SimpleJob]
[MemoryDiagnoser]
public class RefStructInterfaceBenchmark
{
    [Benchmark(Baseline = true)]
    public int RefStructInterface()
    {
        var num = 0;
        for (var i = 0; i < 10_000; i++)
        {
            Aggregate(ref num, new RefStructAge(1));
        }
        return num;
    }

    [Benchmark]
    public int ClassInterface()
    {
        var num = 0;
        for (var i = 0; i < 10_000; i++)
        {
            Aggregate(ref num, new ClassAge(1));
        }
        return num;
    }

    private static void Aggregate<T>(ref int init, T t)
        where T : IAge, allows ref struct
    {
        init += t.GetAge();
    }
}

internal interface IAge
{
    int GetAge();
}

internal readonly ref struct RefStructAge(int age) : IAge
{
    public int GetAge() => age;
}

internal sealed class ClassAge(int age) : IAge
{
    public int GetAge() => age;
}
