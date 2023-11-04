// https://github.com/dotnet/runtime/issues/59019
// https://github.com/dotnet/runtime/pull/84804

using System.Diagnostics;

namespace Net8Sample;

public class ParallelSample
{
    public static async Task MainTest()
    {
        // default parallel options
        Console.WriteLine($"{nameof(Environment.ProcessorCount)}: {Environment.ProcessorCount}");
        Console.WriteLine(TimeProvider.System.GetLocalNow().ToString());
        await Parallel.ForAsync(0, 10, async (_, cancellationToken) =>
        {
            await Task.Delay(1000, cancellationToken); 
        });
        
        Console.WriteLine(TimeProvider.System.GetLocalNow().ToString());
        // MaxDegreeOfParallelism
        await Parallel.ForAsync(0, 10, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 10
            },
            async (_, cancellationToken) =>
            {
                await Task.Delay(1000, cancellationToken);
            });
        Console.WriteLine(TimeProvider.System.GetLocalNow().ToString());
        
        // cancellation
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(300));
        try
        {
            await Parallel.ForAsync(0, 10, new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 5,
                    CancellationToken = cts.Token
                },
                async (_, cancellationToken) =>
                {
                    await Task.Delay(1000, cancellationToken);
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Console.WriteLine(TimeProvider.System.GetLocalNow().ToString());
        
        await Parallel.ForAsync(0, 10, CancellationToken.None, async (i, cancellationToken) =>
        {
            Console.WriteLine(i);
            await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        });
        
        await Parallel.ForEachAsync(Enumerable.Range(0, 10), CancellationToken.None, 
            async (_, cancellationToken) => { await Task.Delay(1000, cancellationToken); });
    }
}
