namespace CSharp15Samples;

internal static class CollectionExpressionPlus
{
    public static void Run()
    {
        List<int> list = [1, 2, 3];
        HashSet<int> set = [1, 2, 3];
        HashSet<string> set2 = ["one", "two", "three"];
        HashSet<string> set3 = [with(StringComparer.OrdinalIgnoreCase), "one", "two", "three", "One"];
        Console.WriteLine("List: " + string.Join(", ", list));
        Console.WriteLine("Set: " + string.Join(", ", set));
        Console.WriteLine("Set2: " + string.Join(", ", set2));
        Console.WriteLine("Set3: " + string.Join(", ", set3));
    }
}
