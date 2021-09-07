# .NET 6 中哈希算法的简化用法

## Intro

微软在 .NET 6 中引入一些更简单的 API 来使用 HMAC 哈希算法（MD5/SHA1/SHA256/SHA384/SHA512)

微软的叫法叫做 `HMAC One-Shoot method`, HMAC 算法在普通的哈希算法基础上增加了一个 key，通过 key 提升了安全性，能够有效避免密码泄露被彩虹表反推出真实密码， JWT(Json Web Token) 除了可以使用 RSA 方式外也支持使用 HMAC 。

## New API

新增的 API 定义如下：

``` c#
namespace System.Security.Cryptography {
    public partial class HMACMD5 {
        public static byte[] HashData(byte[] key, byte[] source);
        public static byte[] HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source);
        public static int HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination);
        public static bool TryHashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten);
    }

    public partial class HMACSHA1 {
        public static byte[] HashData(byte[] key, byte[] source);
        public static byte[] HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source);
        public static int HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination);
        public static bool TryHashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten);
    }

    public partial class HMACSHA256 {
        public static byte[] HashData(byte[] key, byte[] source);
        public static byte[] HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source);
        public static int HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination);
        public static bool TryHashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten);
    }

    public partial class HMACSHA384 {
        public static byte[] HashData(byte[] key, byte[] source);
        public static byte[] HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source);
        public static int HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination);
        public static bool TryHashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten);
    }

    public partial class HMACSHA512 {
        public static byte[] HashData(byte[] key, byte[] source);
        public static byte[] HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source);
        public static int HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination);
        public static bool TryHashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten);
    }
}
```

## Sample Before

在之前的版本中想要实现计算 HMAC 算法会比较复杂，之前实现了一个 `HashHelper` 来封装了常用的 Hash 算法和 HMAC 算法，`HashHelper` 部分代码如下，完整代码可以从 Github 获取：<https://github.com/WeihanLi/WeihanLi.Common/blob/dev/src/WeihanLi.Common/Helpers/HashHelper.cs>

``` c#
/// <summary>
/// 获取哈希之后的字符串
/// </summary>
/// <param name="type">哈希类型</param>
/// <param name="source">源</param>
/// <param name="key">key</param>
/// <param name="isLower">是否是小写</param>
/// <returns>哈希算法处理之后的字符串</returns>
public static string GetHashedString(HashType type, byte[] source, byte[]? key, bool isLower = false)
{
    Guard.NotNull(source, nameof(source));
    if (source.Length == 0)
    {
        return string.Empty;
    }
    var hashedBytes = GetHashedBytes(type, source, key);
    var sbText = new StringBuilder();
    if (isLower)
    {
        foreach (var b in hashedBytes)
        {
            sbText.Append(b.ToString("x2"));
        }
    }
    else
    {
        foreach (var b in hashedBytes)
        {
            sbText.Append(b.ToString("X2"));
        }
    }
    return sbText.ToString();
}

/// <summary>
/// 计算字符串Hash值
/// </summary>
/// <param name="type">hash类型</param>
/// <param name="str">要hash的字符串</param>
/// <returns>hash过的字节数组</returns>
public static byte[] GetHashedBytes(HashType type, string str) => GetHashedBytes(type, str, Encoding.UTF8);

/// <summary>
/// 计算字符串Hash值
/// </summary>
/// <param name="type">hash类型</param>
/// <param name="str">要hash的字符串</param>
/// <param name="encoding">编码类型</param>
/// <returns>hash过的字节数组</returns>
public static byte[] GetHashedBytes(HashType type, string str, Encoding encoding)
{
    Guard.NotNull(str, nameof(str));
    if (str == string.Empty)
    {
        return Array.Empty<byte>();
    }
    var bytes = encoding.GetBytes(str);
    return GetHashedBytes(type, bytes);
}

/// <summary>
/// 获取Hash后的字节数组
/// </summary>
/// <param name="type">哈希类型</param>
/// <param name="bytes">原字节数组</param>
/// <returns></returns>
public static byte[] GetHashedBytes(HashType type, byte[] bytes) => GetHashedBytes(type, bytes, null);

/// <summary>
/// 获取Hash后的字节数组
/// </summary>
/// <param name="type">哈希类型</param>
/// <param name="key">key</param>
/// <param name="bytes">原字节数组</param>
/// <returns></returns>
public static byte[] GetHashedBytes(HashType type, byte[] bytes, byte[]? key)
{
    Guard.NotNull(bytes, nameof(bytes));
    if (bytes.Length == 0)
    {
        return bytes;
    }

    HashAlgorithm algorithm = null!;
    try
    {
        if (key == null)
        {
            algorithm = type switch
            {
                    HashType.SHA1 => new SHA1Managed(),
                    HashType.SHA256 => new SHA256Managed(),
                    HashType.SHA384 => new SHA384Managed(),
                    HashType.SHA512 => new SHA512Managed(),
                    _ => MD5.Create()
            };
        }
        else
        {
            algorithm = type switch
            {
                    HashType.SHA1 => new HMACSHA1(key),
                    HashType.SHA256 => new HMACSHA256(key),
                    HashType.SHA384 => new HMACSHA384(key),
                    HashType.SHA512 => new HMACSHA512(key),
                    _ => new HMACMD5(key)
            };
        }
        return algorithm.ComputeHash(bytes);
    }
    finally
    {
        algorithm.Dispose();
    }
}
```

使用示例如下：

``` c#
HashHelper.GetHashedBytes(HashType.MD5, "test");
HashHelper.GetHashedBytes(HashType.MD5, "test".GetBytes());
HashHelper.GetHashedBytes(HashType.MD5, "test", "testKey");
HashHelper.GetHashedBytes(HashType.MD5, "test".GetBytes(), "testKey".GetBytes());

HashHelper.GetHashedString(HashType.MD5, "test");
HashHelper.GetHashedString(HashType.SHA1, "test".GetBytes());
HashHelper.GetHashedString(HashType.SHA256, "test", "testKey");
HashHelper.GetHashedString(HashType.MD5, "test".GetBytes(), "testKey".GetBytes());
```

## New API Sample

有了新的 API 以后可以怎么简化呢，来看下面的示例：

``` c#
var bytes = "test".GetBytes();
var keyBytes = "test-key".GetBytes();

// HMACMD5
var hmd5V1 = HMACMD5.HashData(keyBytes, bytes);
var hmd5V2 = HashHelper.GetHashedBytes(HashType.MD5, bytes, keyBytes);
Console.WriteLine(hmd5V2.SequenceEqual(hmd5V1));

// HMACSHA1
var hsha1V1 = HMACSHA1.HashData(keyBytes, bytes);
var hsha1V2 = HashHelper.GetHashedBytes(HashType.SHA1, bytes, keyBytes);
Console.WriteLine(hsha1V2.SequenceEqual(hsha1V1));

// HMACSHA256
var hsha256V1 = HMACSHA256.HashData(keyBytes, bytes);
var hsha256V2 = HashHelper.GetHashedBytes(HashType.SHA256, bytes, keyBytes);
Console.WriteLine(hsha256V2.SequenceEqual(hsha256V1));

// HMACSHA384
var hsha384V1 = HMACSHA384.HashData(keyBytes ,bytes);
var hsha384V2 = HashHelper.GetHashedBytes(HashType.SHA384, bytes, keyBytes);
Console.WriteLine(hsha384V2.SequenceEqual(hsha384V1));

// HMACSHA512
var hsha512V1 = HMACSHA512.HashData(keyBytes ,bytes);
var hsha512V2 = HashHelper.GetHashedBytes(HashType.SHA512, bytes, keyBytes);
Console.WriteLine(hsha512V2.SequenceEqual(hsha512V1));
```

直接使用对应的 HMAC 哈希算法的 `HashData` 方法即可，传入对应的 key 和 原始内容就可以了，上面是和我们 `HashHelper` 封装的方法进行对比，看结果是否一致，都是一致的，输出结果如下：

![](C:\Users\Weiha\AppData\Roaming\Typora\typora-user-images\image-20210907235022826.png)

## More

对于普通的哈希算法，微软其实在 .NET 5 就已经支持了上面的用法，可以尝试一下下面的代码：

``` c#
var bytes = "test".GetBytes();

// MD5
var md5V1 = MD5.HashData(bytes);
var md5V2 = HashHelper.GetHashedBytes(HashType.MD5, bytes);
Console.WriteLine(md5V2.SequenceEqual(md5V1));

// SHA1
var sha1V1 = SHA1.HashData(bytes);
var sha1V2 = HashHelper.GetHashedBytes(HashType.SHA1, bytes);
Console.WriteLine(sha1V2.SequenceEqual(sha1V1));

// SHA256
var sha256V1 = SHA256.HashData(bytes);
var sha256V2 = HashHelper.GetHashedBytes(HashType.SHA256, bytes);
Console.WriteLine(sha256V2.SequenceEqual(sha256V1));

// SHA384
var sha384V1 = SHA384.HashData(bytes);
var sha384V2 = HashHelper.GetHashedBytes(HashType.SHA384, bytes);
Console.WriteLine(sha384V2.SequenceEqual(sha384V1));

// SHA512
var sha512V1 = SHA512.HashData(bytes);
var sha512V2 = HashHelper.GetHashedBytes(HashType.SHA512, bytes);
Console.WriteLine(sha512V2.SequenceEqual(sha512V1));
```

很多时候我们可能都会要使用 MD5 或者 SHA1 之后的字符串，不知道为什么微软没有直接获取一个字符串的方法，如果有这样一个方法，就会更方便了，相比之后，感觉还是自己封装的 `HashHelper` 使用起来更舒服一些，哈哈，这样的静态方法不够抽象如果要动态替换哈希算法代码可能就有点...

## References

- <https://github.com/dotnet/runtime/pull/53487>
- <https://github.com/dotnet/runtime/issues/40012>
- <https://github.com/dotnet/core/issues/6569#issuecomment-913876347>
- <https://baike.baidu.com/item/hmac/7307543?fr=aladdin>
- <https://github.com/WeihanLi/SamplesInPractice/blob/master/net6sample/HashSample/Program.cs>
- <https://github.com/WeihanLi/WeihanLi.Common/blob/dev/src/WeihanLi.Common/Helpers/HashHelper.cs>

