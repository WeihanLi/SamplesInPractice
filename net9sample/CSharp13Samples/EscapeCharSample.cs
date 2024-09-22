namespace CSharp13Samples;

/// <summary>
/// \e as a shortcut/short-hand replacement for the character code point 0x1b, commonly known as the ESCAPE (or ESC) character.
/// https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/esc-escape-sequence.md
/// <seealso href="https://grep.app/search?q=%5Cu001b&filter[lang][0]=C%23">grep search result</seealso>
/// <seealso href="https://github.com/search?q=repo%3Aspectreconsole%2Fspectre.console+%5Cu001b&type=code&p=3">Github SpectreConsole SearchResult</seealso>
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
