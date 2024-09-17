using System.Collections;

namespace CSharp13Samples;
internal static class RefStructInterfaceSample
{
    public static void MainTest()
    {
        int[] numbers = [1, 2, 3, 4];
        using var enumerator = GetEnumerator(numbers);
        // CS9244: The type 'ArrayEnumerator<int>' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'RefStructInterfaceSample.Iterate<T>(T)'
        Iterate(enumerator);
    }

    private static ArrayEnumerator<int> GetEnumerator(this int[] items)
    {
        Console.WriteLine("Returning ArrayEnumerator<int>");
        return new ArrayEnumerator<int>(items);
    }

    private static void Iterate<T>(T enumerator)
        where T : IEnumerator<int>
        , allows ref struct
    {
        while (enumerator.MoveNext())
        {
            Console.WriteLine(enumerator.Current);
        }
    }
}

internal ref struct ArrayEnumerator<T>(T[] items) : IEnumerator<T>
{
    private int _idx = -1;

    public T Current => _idx > -1 && _idx < items.Length ? items[_idx] : throw new InvalidOperationException("Current can be accessed only when MoveNext() returns true");

    object? IEnumerator.Current => Current;

    public void Dispose()
    {
        Reset();
    }

    public bool MoveNext()
    {
        if (_idx >= items.Length - 1)
            return false;

        _idx++;
        return true;
    }

    public void Reset()
    {
        _idx = -1;
    }
}
