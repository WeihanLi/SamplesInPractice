namespace CSharp13Samples;

internal static class RefUnsafeExtensionSample
{
    public static async Task MainTest()
    {
        // ref in async method
        // var age = 10;
        // ref int i = age;
        await Task.CompletedTask;
    }


    private static unsafe System.Collections.Generic.IEnumerable<int> M() // an iterator
    {
        yield return 1;
        local();

        async void local0()
        {
            //int* p = null; // allowed in C# 12; error in C# 13 (breaking change, unsafe context needed)        

            await Task.Yield(); // error in C# 12, allowed in C# 13
        }

        async void local()
        {
            unsafe
            {
                int* p = null; // allowed in C# 12; error in C# 13 (breaking change, unsafe context needed)
            }

            await Task.Yield(); // error in C# 12, allowed in C# 13
        }
    }
}
