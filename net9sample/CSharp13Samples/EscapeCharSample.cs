namespace CSharp13Samples;

// Grep usage: https://grep.app/search?q=%5Cu001b&filter[lang][0]=C%23
// GitHub Spectre Console SearchResult: https://github.com/search?q=repo%3Aspectreconsole%2Fspectre.console+%5Cu001b&type=code&p=3
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

        // 
        Console.WriteLine("\u001b[31mThis text has a red foreground using ANSI escape codes.\u001b[0m");
        Console.WriteLine("\e[31mThis text has a red foreground using ANSI escape codes.\e[0m");
    }
}
