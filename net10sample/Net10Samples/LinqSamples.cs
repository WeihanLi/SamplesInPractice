namespace Net10Samples;

public class LinqSamples
{
    public static async Task AsyncSamples()
    {
        var source = Enumerable.Range(1, 5);
        Console.WriteLine(await AsyncEnumerable.Empty<int>().CountAsync());
        
        var a = source.ToAsyncEnumerable();
        var array = await a.ToArrayAsync();
        foreach (var item in array)
        {
            Console.WriteLine(item);
        }
    }

    public static void ShuffleSamples()
    {
        var source = Enumerable.Range(1, 5);
        // source.Shuffle();
    }
}
