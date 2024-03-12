using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

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
            var entryDigestList = new List<EntryHashDigestModel>();
            using var ms = new MemoryStream();
            
            {
                using var zipFile = new ZipArchive(ms, ZipArchiveMode.Create, true);
                var entry = zipFile.CreateEntry("hello.txt");
                await using (var stream = new HashDigestStream(entry.Open()))
                {
                    await using (var fs = File.OpenRead(filePath))
                    {
                        await fs.CopyToAsync(stream);
                    }

                    var entryHash = stream.GetDigestHash();
                    var entryDigest = Convert.ToHexString(entryHash);
                    entryDigestList.Add(new EntryHashDigestModel()
                    {
                        EntryName = entry.Name,
                        HashDigest = entryDigest
                    });
                }
            }
            
            var entryDigestListString = string.Join(";", entryDigestList.OrderBy(x=> x.EntryName)
                .Select(e => $"{e.EntryName}__{e.HashDigest}"));
            var entryDigestListBytes = Encoding.UTF8.GetBytes(entryDigestListString);
            // use lower case to align with container digest
            var combinedDigest = Convert.ToHexString(SHA256.HashData(entryDigestListBytes)).ToLowerInvariant();
            Console.WriteLine(combinedDigest);
        }
    }
}

// inspired by
// https://github.com/dotnet/sdk/blob/188fe2e0a2b8e98eedf03bc796045540009085fa/src/Containers/Microsoft.NET.Build.Containers/Layer.cs#L222
[ExcludeFromCodeCoverage]
file sealed class HashDigestStream(Stream stream, HashAlgorithmName? hashAlgorithmName = null) : Stream
{
    private readonly IncrementalHash _incrementalHash = IncrementalHash.CreateHash(hashAlgorithmName ?? HashAlgorithmName.SHA256);

    public override bool CanWrite => true;

    public override void Write(byte[] buffer, int offset, int count)
    {
        _incrementalHash.AppendData(buffer, offset, count);
        stream.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _incrementalHash.AppendData(buffer);
        stream.Write(buffer);
    }

    public override void Flush() => stream.Flush();

    public byte[] GetDigestHash() => _incrementalHash.GetHashAndReset();

    protected override void Dispose(bool disposing)
    {
        try
        {
            _incrementalHash.Dispose();
            stream.Dispose();
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    // This class is never used with async writes, but if it ever is, implement these overrides
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await stream.WriteAsync(buffer, offset, count, cancellationToken);
        _incrementalHash.AppendData(buffer, offset, count);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await stream.WriteAsync(buffer, cancellationToken);
        _incrementalHash.AppendData(buffer.Span);
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override long Length => throw new NotImplementedException();
    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
    public override void SetLength(long value) => throw new NotImplementedException();
}

file sealed class EntryHashDigestModel
{
    public required string EntryName { get; init; }
    public required string HashDigest { get; init; }
}
