namespace Net9Samples;

// https://github.com/dotnet/runtime/issues/93623
// https://github.com/dotnet/runtime/pull/96732
public class TextWriterBroadcastingSample
{
    public static void MainTest()
    {
        var previousWriter = Console.Out;
        using var textWriter1 = new StringWriter();
        using var newWriter = TextWriter.CreateBroadcasting(previousWriter, textWriter1);
        Console.SetOut(newWriter);
        Console.WriteLine("Hello World");
        
        newWriter.Flush();
        
        Console.WriteLine($"textWriter1: {textWriter1.GetStringBuilder()}");
        textWriter1.GetStringBuilder().Clear();
        
        Console.SetOut(previousWriter);
        
        Console.WriteLine("Hello .NET");
        Console.WriteLine($"textWriter1: {textWriter1.GetStringBuilder()}");

        Console.WriteLine("Completed");
    }
}
