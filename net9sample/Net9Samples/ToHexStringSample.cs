using System.Security.Cryptography;

namespace Net9Samples;

public static class ToHexStringSample
{
    public static void MainTest()
    {
        var source = "Hello World"u8;
        var hashBytes = MD5.HashData(source);

        Console.WriteLine("BitConverter");
        Console.WriteLine(BitConverter.ToString(hashBytes));
        Console.WriteLine(BitConverter.ToString(hashBytes).Replace("-", ""));

        Console.WriteLine("string.Join ToString");
        Console.WriteLine(string.Join("", hashBytes.Select(x => x.ToString("X2"))));
        Console.WriteLine(string.Join("", hashBytes.Select(x => x.ToString("x2"))));

        Console.WriteLine("ToHexString");
        Console.WriteLine(Convert.ToHexString(hashBytes));
        Console.WriteLine(Convert.ToHexStringLower(hashBytes));
        
        var bytes1 = Convert.FromHexString(Convert.ToHexString(hashBytes));
        var bytes2 = Convert.FromHexString(Convert.ToHexStringLower(hashBytes));
        Console.WriteLine(bytes1.SequenceEqual(bytes2));
    }
}
