using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace CSharp12Sample;

// https://github.com/dotnet/runtime/blob/51ccb5f6553308845aeb24672504b6f59c24b4c9/src/libraries/System.Linq.Expressions/src/System/Runtime/CompilerServices/ReadOnlyCollectionBuilder.cs#L15
// https://github.com/dotnet/runtime/blob/51ccb5f6553308845aeb24672504b6f59c24b4c9/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/CollectionBuilderAttribute.cs#L12
// https://github.com/dotnet/csharplang/blob/main/proposals/csharp-12.0/collection-expressions.md
// https://github.com/dotnet/csharplang/issues/5354
// https://github.com/dotnet/roslyn/issues/68785
// https://github.com/dotnet/roslyn/issues/69950
public class CollectionLiteralSample
{
    public static void MainTest()
    {
        // error CS9176: There is no target type for the collection expression.
        // var empty = [];

        int[] numArray = [1, 2, 3];
        HashSet<int> numSet = [1, 2, 3];
        List<int> numList = [1, 2, 3];
        Span<char> charSpan = ['H', 'e', 'l', 'l', 'o'];
        ReadOnlySpan<string> stringSpan = ["Hello", "World"];
        ImmutableArray<string> immutableArray = ["Hello", "World"];
        
        
        int[] nums = [1, 1, ..numArray, 2, 2];
        Console.WriteLine(string.Join(",", nums));
        Console.ReadLine();

        int[] row0 = [1, 2, 3];
        int[] row1 = [4, 5, 6];
        int[] row2 = [7, 8, 9];
        int[] single = [..row0, ..row1, ..row2];
        foreach (var element in single)
        {
            Console.Write($"{element}, ");
        }
        System.Console.WriteLine();
        System.Console.WriteLine();
        
        IEnumerable<string> enumerable = ["Hello", "dotnet"];
        System.Console.WriteLine(enumerable.GetType());
        IReadOnlyCollection<string> readOnlyCollection = ["Hello", "dotnet"];
        System.Console.WriteLine(readOnlyCollection.GetType());
        ICollection<string> collection = ["Hello", "dotnet"];
        System.Console.WriteLine(collection.GetType());
        
        // ISet<string> set = ["Hello", "dotnet"];
        // System.Console.WriteLine(set.GetType());
        // IReadOnlySet<string> readonlySet = ["Hello", "dotnet"];
        // System.Console.WriteLine(readonlySet.GetType());

        IReadOnlyList<string> readOnlyList = ["Hello", "dotnet"];
        System.Console.WriteLine(readOnlyList.GetType());
        IList<string> list = ["Hello", "dotnet"];
        System.Console.WriteLine(list.GetType());
        
        IEnumerable<string> emptyEnumerable = [];
        System.Console.WriteLine(emptyEnumerable.GetType());
        IReadOnlyCollection<string> emptyReadonlyCollection = [];
        System.Console.WriteLine(emptyReadonlyCollection.GetType());
        ICollection<string> emptyCollection = [];
        System.Console.WriteLine(emptyCollection.GetType());
        IReadOnlyList<string> emptyReadOnlyList = [];
        System.Console.WriteLine(emptyReadOnlyList.GetType());
        IList<string> emptyList = [];
        System.Console.WriteLine(emptyList.GetType());
        System.Console.WriteLine();
        
        Console.ReadLine();
        CustomNumberCollection customNumberCollection = [1, 2, 3];
        System.Console.WriteLine(string.Join(",", customNumberCollection.Numbers));

        CustomCollection<string> customCollection = [ "Hello", "World" ];
        System.Console.WriteLine(string.Join(",", customCollection.Elements));
    }
}

[CollectionBuilder(typeof(CustomCollectionBuilder), nameof(CustomCollectionBuilder.CreateNumber))]
file sealed class CustomNumberCollection : IEnumerable<int>
{
    public required int[] Numbers { get; init; }
    public IEnumerator<int> GetEnumerator()
    {
        return (IEnumerator<int>)Numbers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Numbers.GetEnumerator();
    }
}

[CollectionBuilder(typeof(CustomCollectionBuilder), nameof(CustomCollectionBuilder.Create))]
file sealed class CustomCollection<T>
{
    public required T[] Elements { get; init; }

    public IEnumerator<T> GetEnumerator()
    {
        return (IEnumerator<T>)Elements.GetEnumerator();
    }
}

file static class CustomCollectionBuilder
{
    // public static IEnumerator<T> GetEnumerator<T>(this CustomCollection<T> collection) 
    //     => (IEnumerator<T>)collection.Elements.GetEnumerator();
    
    public static CustomNumberCollection CreateNumber(ReadOnlySpan<int> elements)
    {
        return new CustomNumberCollection()
        {
            Numbers = elements.ToArray()
        };
    }

    public static CustomCollection<T> Create<T>(ReadOnlySpan<T> elements)
    {
        return new CustomCollection<T>()
        {
            Elements = elements.ToArray()
        };
    }
}
