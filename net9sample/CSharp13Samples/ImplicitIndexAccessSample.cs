namespace CSharp13Samples;

// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#implicit-index-access
internal class ImplicitIndexAccessSample
{
    public static void MainTest()
    {
        var a = new TestClass()
        {
            Numbers =
            {
                [0] = 3,
                [^1] = 1
            },
            NumList =
            {
                [^1] = 2
            }
        };

        Console.WriteLine(nameof(a.Numbers));
        foreach (var item in a.Numbers)
        {
            Console.WriteLine(item);
        }

        Console.WriteLine(nameof(a.NumList));
        foreach (var item in a.NumList)
        {
            Console.WriteLine(item);
        }
    }
}

file sealed class TestClass
{
    public int[] Numbers { get; init; } = new int[3];

    public List<int> NumList { get; init; } = new([1, 1, 2]);
}
