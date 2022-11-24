namespace CSharp11Sample;
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#utf-8-string-literals
public class Utf8StringLiteralSample
{
    public static void MainTest()
    {
        var helloBytes = "Hello"u8;        
        Console.WriteLine(Encoding.UTF8.GetString(helloBytes));

        var authStringLiteral = "AUTH "u8.ToArray();
        Console.WriteLine(authStringLiteral.GetType().FullName);
        Console.WriteLine(Encoding.UTF8.GetString(authStringLiteral));
    }
}
