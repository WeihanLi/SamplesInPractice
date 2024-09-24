namespace Net9Samples;
internal static class InterlockedSample
{
    public static void MainTest()
    {
        var a = 1;
        var original = Interlocked.CompareExchange(ref a, 2, 0);
        Console.WriteLine(a);
        Console.WriteLine(original);
    }
}
