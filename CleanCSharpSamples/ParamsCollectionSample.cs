namespace CleanCSharpSamples;

public class ParamsCollectionSample
{
    public static void Run()
    {
        ParamsArrayMethod(1, 2, 3);
        ParamsListMethod(1, 2, 3);
        ParamsEnumerableMethod(1, 2, 3);
        ParamsSpanMethod(1, 2, 3);
        ParamsReadOnlySpanMethod(1, 2, 3);
    }
    
    private static void ParamsArrayMethod(params int[] array)
    {
        foreach (var item in array)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsReadOnlySpanMethod(params ReadOnlySpan<int> collection)
    {
        foreach (var item in collection)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsSpanMethod(params Span<int> collection)
    {
        foreach (var item in collection)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsListMethod(params List<int> list)
    {
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
    }

    private static void ParamsEnumerableMethod(params IEnumerable<int> array)
    {
        foreach (var item in array)
        {
            Console.WriteLine(item);
        }
    }
}
