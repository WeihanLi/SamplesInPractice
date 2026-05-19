using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;

namespace Net10Samples;

public class RandomSample
{
    // https://github.com/dotnet/runtime/issues/111956
    // https://github.com/dotnet/runtime/pull/112162
    public static void Run()
    {
        Console.WriteLine("GetString");
        var nums = "1234567890";
        var randomString = Random.Shared.GetString(nums, 8);
        Console.WriteLine(randomString);

        Console.WriteLine("GetHexString");
        var randomHexString = Random.Shared.GetHexString(16);
        Console.WriteLine(randomHexString);
        Console.WriteLine(Random.Shared.GetHexString(16, true));

        Console.WriteLine("GetHexString Span");
        Span<char> chars = stackalloc char[16];
        Random.Shared.GetHexString(chars);
        Console.WriteLine(chars);
        Random.Shared.GetHexString(chars, true);
        Console.WriteLine(chars);
        
        Console.WriteLine("RandomNumberGenerator");
        var randString = RandomNumberGenerator.GetString(nums, 8);
        Console.WriteLine(randString);
        Console.WriteLine(RandomNumberGenerator.GetHexString(16));
        Console.WriteLine(RandomNumberGenerator.GetHexString(16, true));
        RandomNumberGenerator.GetHexString(chars);
        Console.WriteLine(chars);
        RandomNumberGenerator.GetHexString(chars, true);
        Console.WriteLine(chars);
    }

    public static void RunBenchmark() => BenchmarkRunner.Run<Benchmark>();

    [ShortRunJob]
    public class Benchmark
    {
        private static readonly char[] Chars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
        
        [Benchmark(Baseline = true)]
        public string RandomGetString()
        {
            return Random.Shared.GetString(Chars, 8);
        }
        
        
        [Benchmark]
        public string RandomNumberGeneratorGetString()
        {
            return RandomNumberGenerator.GetString(Chars, 8);
        }
    }
}
