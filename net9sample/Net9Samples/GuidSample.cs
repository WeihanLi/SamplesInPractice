using System.Text;

namespace Net9Samples;
internal static class GuidSample
{
    // https://datatracker.ietf.org/doc/rfc9562/
    // https://www.rfc-editor.org/rfc/rfc9562.html
    // https://github.com/dotnet/runtime/issues/103658
    // https://github.com/dotnet/runtime/pull/104124
    public static void MainTest()
    {
        Console.WriteLine(string.Join("-", Guid.Empty.ToByteArray().Select(b=> b.ToString())));
        Console.WriteLine(string.Join("-", Guid.AllBitsSet.ToByteArray().Select(b=> b.ToString())));

        var guid = Guid.CreateVersion7();
        Console.WriteLine(guid.ToString());
        Console.WriteLine($"{nameof(guid.Version)}: {guid.Version}");
        Console.WriteLine($"{nameof(guid.Variant)}: {guid.Variant}");
        
        var timestamp = DateTimeOffset.UtcNow;
        Console.WriteLine($"Timestamp: {timestamp} {timestamp.ToUnixTimeMilliseconds()}");
        Console.WriteLine(Guid.CreateVersion7(timestamp));

        guid = Guid.CreateVersion7(timestamp);
        Console.WriteLine(guid);

        Thread.Sleep(2000);
        Console.WriteLine(Guid.CreateVersion7());
        
        PrintDateTime(guid);
    }

    private static void PrintDateTime(Guid guid)
    {
        if (guid.Version is not 7)
        {
            throw new InvalidOperationException("Guid.Version is not 7");
        }

        var bytes = guid.ToByteArray();
        var a = BitConverter.ToInt32(bytes.AsSpan(0, 4));
        var b = BitConverter.ToInt16(bytes.AsSpan(4, 2));
        var timestamp = (((long)a) << 16) + b; 
        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        Console.WriteLine($"DateTime: {dateTime.UtcDateTime}   {timestamp}");
    }
}
