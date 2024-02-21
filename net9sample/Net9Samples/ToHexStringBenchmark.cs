using BenchmarkDotNet.Attributes;
using System.Security.Cryptography;

namespace Net9Samples;

[MemoryDiagnoser]
public class ToHexStringBenchmark
{
    private readonly byte[] _hashBytes = MD5.HashData("Hello World"u8);

    [Benchmark(Baseline = true)]
    public string ToHexString() => Convert.ToHexString(_hashBytes);
    
    [Benchmark]
    public string ToStringJoin() => string.Join("", _hashBytes.Select(x => x.ToString("X2")));
    
    [Benchmark]
    public string BitConverterToString() => BitConverter.ToString(_hashBytes).Replace("-", "");
    
    [Benchmark]
    public string ToHexStringLower() => Convert.ToHexStringLower(_hashBytes);
    
    [Benchmark]
    public string ToStringJoinLower() => string.Join("", _hashBytes.Select(x => x.ToString("x2")));
    
    [Benchmark]
    public string BitConverterToStringLower() => BitConverter.ToString(_hashBytes).Replace("-", "").ToLower();
    
    [Benchmark]
    public string ToHexStringThenLower() => Convert.ToHexString(_hashBytes).ToLowerInvariant();
}
