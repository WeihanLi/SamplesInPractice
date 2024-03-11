using System.IO.Compression;
using System.Security.Cryptography;

namespace BalabalaSample;

public class ZipCheckSumSample
{
    public static async Task MainTest()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "hello.txt");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        await File.WriteAllTextAsync(filePath, "Hello World");

        await GetSHA256Digest();
        await Task.Delay(2000);
        await GetSHA256Digest();
        
        async Task GetSHA256Digest()
        {
            using var ms = new MemoryStream();
            using var zipFile = new ZipArchive(ms, ZipArchiveMode.Create, true);
            var entry = zipFile.CreateEntry("hello.txt");
            await using var stream = entry.Open();
            await using (var fs = File.OpenRead(filePath))
            {
                await fs.CopyToAsync(stream);
            }

            ms.Seek(0, SeekOrigin.Begin);
            var hashBytes = await SHA256.HashDataAsync(ms);
            Console.WriteLine(Convert.ToHexString(hashBytes));
        }
    }
}
