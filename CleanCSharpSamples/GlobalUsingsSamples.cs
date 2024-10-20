using System.Collections.Concurrent;

namespace CleanCSharpSamples;

public static class GlobalUsingsSamples
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Cache
        = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
    
    // global using alias + target-typed new expression
    private static readonly MyCache Cache1 = new();

    public static void UsingSamples()
    {
        {
            System.Console.WriteLine("Hello Static Using!");
            System.Console.WriteLine(System.Environment.Version.ToString());
        }

        {
            // global using static
            WriteLine("Hello Static Using!");

            // global using alias
            WriteLine(Runtime.Version.ToString());
        }
    }
}
