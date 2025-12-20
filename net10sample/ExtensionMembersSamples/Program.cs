using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

// var bytes = "Hello, World!"u8.ToArray();
// Console.WriteLine(bytes.CustomHexStringLower());
// Console.WriteLine(Convert.ToHexStringLower(bytes));

BenchmarkRunner.Run<ExtensionPerfTest>();

[ShortRunJob(RuntimeMoniker.Net80)]
[ShortRunJob(RuntimeMoniker.Net90)]
public class ExtensionPerfTest
{
    private readonly byte[] bytes = "Performance Test String"u8.ToArray();
    
    [Benchmark(Baseline = true)]
    public string ToHexStringExtMember()
    {
        return Convert.ToHexStringLower(bytes);
    }

    [Benchmark]
    public string ToHexStringExtMethod()
    {
        return bytes.CustomHexStringLower();
    }
}
