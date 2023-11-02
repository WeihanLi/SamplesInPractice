// https://github.com/dotnet/runtime/pull/87067
// https://github.com/dotnet/runtime/pull/87067/files#diff-55c104dbddd158b7d649d07c342facc10f353b1b036bb35a0ad71625073eb637
// https://github.com/dotnet/runtime/pull/93949

namespace Net8Sample;

public static class ConfigureAwaitOptionsSample
{
    public static async Task MainTest()
    {
        DumpThreadInfo();
        await ForceYielding();
        await SuppressThrowing();

        Console.ReadLine();
    }

    private static async Task ForceYielding()
    {
        DumpThreadInfo();

        await Task.CompletedTask;
        DumpThreadInfo();
        
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        DumpThreadInfo();
    }

    private static async Task SuppressThrowing()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);
        try
        {
            await Task.Delay(1000, cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        try
        {
            await Task.Delay(1000, cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        var startTimestamp = TimeProvider.System.GetTimestamp();
        
        await Task.Delay(1000, cts.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        
        Console.WriteLine($"{TimeProvider.System.GetElapsedTime(startTimestamp).TotalMicroseconds} ms");


        try
        {
            await ThrowingTask();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        await ThrowingTask().ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        Console.WriteLine("Yeah");

        try
        { 
            // CA2261: The ConfigureAwaitOptions.SuppressThrowing is only supported with the non-generic Task
            await ThrowingTaskWithReturnValue().ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static async Task ContinueOnCapturedContextOrNot()
    {
        await Task.Delay(1000).ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(ConfigureAwaitOptions.None);
        
        await Task.Delay(1000).ConfigureAwait(true);
        await Task.Delay(1000).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
    }

    private static async Task ThrowingTask()
    {
        await Task.Delay(100);
        throw new InvalidOperationException("Balabala");
    }
    
    private static async Task<int> ThrowingTaskWithReturnValue()
    {
        await Task.Delay(100);
        throw new InvalidOperationException("Balabala2");
    }
    
    private static void DumpThreadInfo()
    {
        Console.WriteLine($"ThreadId: {Environment.CurrentManagedThreadId}");
    }
}
