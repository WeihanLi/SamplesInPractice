using System.Runtime.CompilerServices;

namespace CSharp12Sample;

public static class InlineArraySample
{
    public static void MainTest()
    {
        var arr = new MyArray();
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = i;
        }
        foreach (var i in arr)
        {
            Console.Write(i);
            Console.Write(",");
        }
        
        Console.WriteLine();
        
        ReadOnlySpan<int> span = arr;
        foreach (var i in span)
        {
            Console.Write(i);
            Console.Write(",");
        }
        
        Console.WriteLine();
        
        foreach (var i in arr[^2..])
        {
            Console.Write(i);
            Console.Write(",");
        }
        Console.WriteLine();
        
        Console.WriteLine(arr[^1]);

        // error CS0021: Cannot apply indexing with [] to an expression of type 'MyArray'
        // if (arr is [0,1,..])
        //     Console.WriteLine("StartsWith 0, 1");
        
        // error CS9174: Cannot initialize type 'MyArray' with a collection expression because the type is not constructible.
        // arr = [1, 2, 3, 4, 5];
    }
}

[InlineArray(10)]
file struct MyArray
{
    private int _element;

    public int Length => 10;
}
