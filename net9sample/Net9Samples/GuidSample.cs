namespace Net9Samples;
internal static class GuidSample
{
    // https://datatracker.ietf.org/doc/rfc9562/
    // https://www.rfc-editor.org/rfc/rfc9562.html
    // https://github.com/dotnet/runtime/issues/103658
    // https://github.com/dotnet/runtime/pull/104124
    public static void MainTest()
    {
        var guid = Guid.CreateVersion7();
        Console.WriteLine(guid.ToString());
    }
}
