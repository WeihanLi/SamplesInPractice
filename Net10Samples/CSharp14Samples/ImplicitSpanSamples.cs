namespace CSharp14Samples;

public class ImplicitSpanSamples
{
    public static void Run()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        Span<int> span = array;
        Console.WriteLine(span.Length);
        ReadOnlySpan<int> span1 = array;
        Console.WriteLine(span1.Length);
    }
}
