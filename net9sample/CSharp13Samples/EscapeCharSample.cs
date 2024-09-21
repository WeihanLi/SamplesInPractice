namespace CSharp13Samples;

/// <summary>
/// \e as a shortcut/short-hand replacement for the character code point 0x1b, commonly known as the ESCAPE (or ESC) character.
/// https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/esc-escape-sequence.md
/// </summary>
public static class EscapeCharSample
{
    public static void MainTest()
    {
        var escapeChar = '\e';
        var unicodeEscapeChar = '\u001b';

        Console.WriteLine(escapeChar == unicodeEscapeChar);

        Console.WriteLine((int)escapeChar);
        Console.WriteLine((int)unicodeEscapeChar);
    }
}
