using System.Security.Cryptography;

namespace Net9Samples;

// https://github.com/dotnet/runtime/issues/91407
// https://github.com/dotnet/runtime/pull/92430
public static class HashSample
{
    public static void MainTest()
    {
        var helloBytes = "Hello World"u8;
        var keyBytes = "keys"u8;
        
        var sha1Bytes = SHA1.HashData(helloBytes);
        var sha1HashedBytes = CryptographicOperations.HashData(HashAlgorithmName.SHA1, helloBytes);
        Console.WriteLine(sha1HashedBytes.SequenceEqual(sha1Bytes));
        
        var sha1MacBytes = HMACSHA1.HashData(keyBytes, helloBytes);
        var sha1HmacBytes = CryptographicOperations.HmacData(HashAlgorithmName.SHA1, keyBytes, helloBytes);
        Console.WriteLine(sha1HmacBytes.SequenceEqual(sha1MacBytes));
        
        PrintHashString(HashAlgorithmName.MD5, helloBytes);
        PrintHashString(HashAlgorithmName.SHA1, helloBytes);
        PrintHashString(HashAlgorithmName.SHA256, helloBytes);
        PrintHashString(HashAlgorithmName.SHA384, helloBytes);
        PrintHashString(HashAlgorithmName.SHA512, helloBytes);
        // sha3, https://cryptobook.nakov.com/cryptographic-hash-functions/secure-hash-algorithms
        PrintHashString(HashAlgorithmName.SHA3_256, helloBytes);
        PrintHashString(HashAlgorithmName.SHA3_384, helloBytes);
        PrintHashString(HashAlgorithmName.SHA3_512, helloBytes);
        
        // hmac
        Console.WriteLine();
        Console.WriteLine("HMAC samples:");
        PrintHmacString(HashAlgorithmName.MD5, keyBytes, helloBytes);
        PrintHmacString(HashAlgorithmName.SHA1, keyBytes, helloBytes);
        PrintHmacString(HashAlgorithmName.SHA256, keyBytes, helloBytes);
        PrintHmacString(HashAlgorithmName.SHA384, keyBytes, helloBytes);
        PrintHmacString(HashAlgorithmName.SHA512, keyBytes, helloBytes);
        PrintHmacString(HashAlgorithmName.SHA3_256, keyBytes, helloBytes);
        PrintHmacString(HashAlgorithmName.SHA3_384, keyBytes, helloBytes);
        PrintHmacString(HashAlgorithmName.SHA3_512, keyBytes, helloBytes);

        Console.WriteLine("Completed");
    }

    private static void PrintHashString(HashAlgorithmName hashAlgorithmName, ReadOnlySpan<byte> bytes)
    {
        var hashedBytes = CryptographicOperations.HashData(hashAlgorithmName, bytes);
        var hexString = Convert.ToHexString(hashedBytes);
        Console.WriteLine($"{hashAlgorithmName}, {hexString}");
    }
    
    private static void PrintHmacString(HashAlgorithmName hashAlgorithmName, ReadOnlySpan<byte> key, ReadOnlySpan<byte> bytes)
    {
        var hashedBytes = CryptographicOperations.HmacData(hashAlgorithmName, key, bytes);
        var hexString = Convert.ToHexString(hashedBytes);
        Console.WriteLine($"{hashAlgorithmName}, {hexString}");
    }
}
