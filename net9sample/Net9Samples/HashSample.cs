using System.Security.Cryptography;

namespace Net9Samples;

// https://github.com/dotnet/runtime/issues/91407
// https://github.com/dotnet/runtime/pull/92430
public static class HashSample
{
    public static void MainTest()
    {
        var helloBytes = "Hello World"u8;
        var sha1Bytes = SHA1.HashData(helloBytes);

        var hashedBytes = CryptographicOperations.HashData(HashAlgorithmName.SHA1, helloBytes);
        Console.WriteLine(hashedBytes.SequenceEqual(sha1Bytes));
        
        var keysBytes = "keys"u8;
        var hmacBytes = CryptographicOperations.HmacData(HashAlgorithmName.SHA1, keysBytes, helloBytes);
        Console.WriteLine(Convert.ToHexString(hmacBytes));
    }
}
