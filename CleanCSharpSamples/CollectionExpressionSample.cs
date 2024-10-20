using System.Collections.Immutable;

namespace CleanCSharpSamples;

public static class CollectionExpressionSample
{
    public static void Run()
    {
        {
            int[] numArray = [1, 2, 3];
            HashSet<int> numSet = [1, 2, 3];
            List<int> numList = [1, 2, 3];
            Span<char> charSpan = ['H', 'e', 'l', 'l', 'o'];
            ReadOnlySpan<string> stringSpan = ["Hello", "World"];
            ImmutableArray<string> immutableArray = ["Hello", "World"];


            int[] nums = [1, 1, ..numArray, 2, 2];
            Console.WriteLine(string.Join(",", nums));

            int[] row0 = [1, 2, 3];
            int[] row1 = [4, 5, 6];
            int[] row2 = [7, 8, 9];
            int[] single = [..row0, ..row1, ..row2];
            foreach (var element in single)
            {
                Console.Write($"{element}, ");
            }
            Console.WriteLine();
        }

        {
            IEnumerable<string> enumerable = ["Hello", "dotnet"];
            Console.WriteLine(enumerable.GetType());
            IReadOnlyCollection<string> readOnlyCollection = ["Hello", "dotnet"];
            Console.WriteLine(readOnlyCollection.GetType());
            ICollection<string> collection = ["Hello", "dotnet"];
            Console.WriteLine(collection.GetType());

            IReadOnlyList<string> readOnlyList = ["Hello", "dotnet"];
            Console.WriteLine(readOnlyList.GetType());
            IList<string> list = ["Hello", "dotnet"];
            Console.WriteLine(list.GetType());
        
            IEnumerable<string> emptyEnumerable = [];
            Console.WriteLine(emptyEnumerable.GetType());
            IReadOnlyCollection<string> emptyReadonlyCollection = [];
            Console.WriteLine(emptyReadonlyCollection.GetType());
            ICollection<string> emptyCollection = [];
            Console.WriteLine(emptyCollection.GetType());
            IReadOnlyList<string> emptyReadOnlyList = [];
            Console.WriteLine(emptyReadOnlyList.GetType());
            IList<string> emptyList = [];
            Console.WriteLine(emptyList.GetType());
        }
    }
}
