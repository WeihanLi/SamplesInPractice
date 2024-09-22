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
        var age = new RefStructAge();
        return age.GetAge();
    }

    [Benchmark]
    public int ClassInterface()
    {
        var age = new ClassAge();
        return age.GetAge();
    }
}

internal interface IAge
{
    int GetAge();
}

internal ref struct RefStructAge : IAge
{
    public int GetAge() => 1;
}

internal sealed class ClassAge : IAge
{
    public int GetAge() => 1;
}
