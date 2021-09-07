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

Console.WriteLine();

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

Console.WriteLine("Hello, World!");
Console.ReadLine();
