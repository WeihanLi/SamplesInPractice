namespace CSharp14Samples;

public static class ExtensionSamples
{
    public static void Run()
    {
        var nums = Enumerable.Range(1, 10);
        Console.WriteLine(nums.IsEmpty);
        
        foreach (var num in nums.WhereGreaterThan(5))
        {
            Console.WriteLine(num);
        }

        var list = List<int>.New();
        var list2 = List<int>.New(2);

        var emptyArray = ICollection<int>.Empty;

        // extension indexer not supported yet
        // https://github.com/dotnet/csharplang/discussions/8696#discussioncomment-12814338
        // ICollection<int> numCollection = nums.ToArray().AsReadOnly();
        // Console.WriteLine(numCollection[Random.Shared.Next(0, numCollection.Count)]);

        //
        ExtensionSamples.AnotherRun();
    }
}

file static class Extensions
{
    public static IEnumerable<int> WhereGreaterThan1(this IEnumerable<int> source, int threshold) 
        => source.Where(x => x > threshold);
    
    extension(IEnumerable<int> source) 
    {
        public IEnumerable<int> WhereGreaterThan(int threshold)
        {
            Console.WriteLine("Extensions.WhereGreaterThan");
            return source.Where(x => x > threshold);
        }
        
        public bool IsEmpty
                => !source.Any();
    }

    extension(ExtensionSamples)
    {
        public static void AnotherRun()
        {
            Console.WriteLine("AnotherRun");
        }
    }

    extension<T>(List<T>)
    {
        public static List<T> New() => [];

        public static List<T> New(int capacity) => new(capacity);
    }

    extension<T>(ICollection<T>)
    {
        public static T[] Empty => Array.Empty<T>();
    }
    
    extension<T>(ICollection<T> source)
    {
        // https://github.com/dotnet/csharplang/discussions/8696#discussioncomment-12814338
        // public T this[int index]
        // {
        //     get
        //     {
        //         ArgumentOutOfRangeException.ThrowIfNegative(index);
        //         ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, source.Count);
        //         return source.ElementAt(index);
        //     }
        // }
    }
}
