using System.Collections.Frozen;

namespace Net8Sample;

public static class FrozenCollectionSample
{
    public static void MainTest()
    {
        var elements = new[] { "Hello", "hello", "World" };
        var set = elements.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        Console.WriteLine(set.Contains("Hello"));

        Console.WriteLine(set.IsSubsetOf(new[] { "Hello", "World" }));
        Console.WriteLine(set.IsSupersetOf(new[] { "Hello" }));
        Console.WriteLine(set.IsProperSupersetOf(new[] { "Hello" }));
        Console.WriteLine(set.IsProperSubsetOf(new[] { "Hello", "World", "Test" }));

        var dic = elements.ToFrozenDictionary(x => x, x => x[..1]);
        var val = dic.GetValueRefOrNullRef("hello");
        Console.WriteLine(val);
    }
}
