using System.Threading.Channels;

namespace Net9Samples;

// https://github.com/dotnet/runtime/issues/62761
// https://github.com/dotnet/runtime/pull/100550
public static class PriorityChannelSample
{
    public static async Task MainTestAsync()
    {
        {
            var c = Channel.CreateUnboundedPrioritized<int>();

            await c.Writer.WriteAsync(1);
            await c.Writer.WriteAsync(5);
            await c.Writer.WriteAsync(2);
            await c.Writer.WriteAsync(4);
            await c.Writer.WriteAsync(3);
            c.Writer.Complete();

            while (await c.Reader.WaitToReadAsync())
            {
                while (c.Reader.TryRead(out int item))
                {
                    Console.WriteLine($"{item} ");
                }
            }
            // Output: 1 2 3 4 5
        }
        Console.WriteLine(new string('-', 50));
        {
            var c = Channel.CreateUnboundedPrioritized<int>(new()
            {
                Comparer = new ReverseComparer()
            });

            await c.Writer.WriteAsync(1);
            await c.Writer.WriteAsync(5);
            await c.Writer.WriteAsync(2);
            await c.Writer.WriteAsync(4);
            await c.Writer.WriteAsync(3);
            c.Writer.Complete();

            while (await c.Reader.WaitToReadAsync())
            {
                while (c.Reader.TryRead(out int item))
                {
                    Console.WriteLine($"{item} ");
                }
            }
            // Output: 5 4 3 2 1
        }

        Console.WriteLine("Completed");
    }
}

file sealed class ReverseComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        return y.CompareTo(x);
    }
}
