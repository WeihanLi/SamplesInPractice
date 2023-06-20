using System.Buffers;

namespace Net8Sample;

public class IndexOfAnyValueSample
{
    public static void MainTest()
    {
        // ReSharper disable StringLiteralTypo
        var chars = "-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz".ToCharArray();
        var i = "Hello".IndexOfAny(chars);
        Console.WriteLine(i);
        
        var chars2 = SearchValues.Create("-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz");
        var i2 = "Hello".AsSpan().IndexOfAny(chars2);
        Console.WriteLine(i2);
    }
}
