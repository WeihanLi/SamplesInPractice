using System.Runtime.CompilerServices;

namespace CSharp15Samples;

internal static class CollectionExpressionArguments
{
    public static void Run()
    {
        List<int> list = [with(capacity: 3), 1, 2, 3];
        HashSet<int> set = [1, 2, 3];
        HashSet<string> set2 = ["one", "two", "three"];
        HashSet<string> set3 = [with(StringComparer.OrdinalIgnoreCase), .. set2, "One"];
        Console.WriteLine("List: " + string.Join(", ", list) + $" {nameof(list.Capacity)}: {list.Capacity}");
        Console.WriteLine("Set: " + string.Join(", ", set));
        Console.WriteLine("Set2: " + string.Join(", ", set2));
        Console.WriteLine("Set3: " + string.Join(", ", set3));

        Console.WriteLine();
        MyCollection<string> myCollection = [];
        MyCollection<int> myCollection2 = [1, 2, 3];
        MyCollection<int> myCollection3 = [with(3), 1, 2];
        Console.WriteLine($"myCollection3: {string.Join(", ", myCollection3.Items)}");
    }
}

[CollectionBuilder(typeof(MyCollectionBuilder), "Create")]
internal sealed class MyCollection<T>
{
    public required T[] Items { get; init; }

    public IEnumerator<T> GetEnumerator()
    {
        return (IEnumerator<T>)Items.GetEnumerator();
    }
}

internal sealed class MyCollectionBuilder
{
    public static MyCollection<T> Create<T>(ReadOnlySpan<T> items)
    {
        return new MyCollection<T> { Items = items.ToArray() };
    }

    public static MyCollection<T> Create<T>(int length, ReadOnlySpan<T> items)
    {
        var array = new T[length];
        var i = 0;
        foreach (var item in items)
        {
            array[i++] = item;
        }
        return new MyCollection<T> { Items = array };
    }
}
