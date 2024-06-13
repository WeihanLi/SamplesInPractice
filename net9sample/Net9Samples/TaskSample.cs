namespace Net9Samples;

// https://github.com/dotnet/runtime/issues/47998
// https://github.com/dotnet/runtime/pull/97077
public static class TaskSample
{
    public static async Task MainTest()
    {
        {
            var tcs = new TaskCompletionSource();
            var startTimestamp = TimeProvider.System.GetTimestamp();
            _ = Task.Delay(2000).ContinueWith(r => tcs.SetFromTask(r));
            await tcs.Task;
            var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);
            Console.WriteLine($"Completed in {elapsedTime.TotalMilliseconds}ms");
        }
        
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetFromTask(Task.FromResult(100));
            Console.WriteLine(await tcs.Task);
            Console.WriteLine("Completed");
        }
        
        {
            var tcs = new TaskCompletionSource();
            using var cts = new CancellationTokenSource(200);
            var task = Task.Delay(2000, cts.Token);
            await Task.Delay(500);
            try
            {
                tcs.SetFromTask(task);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(tcs.Task.Status);
                Console.WriteLine("Exception:");
                Console.WriteLine(ex);
            }
            
            Console.WriteLine("Completed");
        }

        {
            var tcs = new TaskCompletionSource();
            var task = Task.Run(() =>
            {
                throw new NotImplementedException();
            });
            await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            try
            {
                tcs.SetFromTask(task);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(tcs.Task.Status);
                Console.WriteLine("Exception:");
                Console.WriteLine(ex);
            }
            
            Console.WriteLine("Completed");
        }

        
        {
            var tcs = new TaskCompletionSource();
            tcs.SetCanceled();
            Console.WriteLine(tcs.Task.Status);
            try
            {
                Console.WriteLine(tcs.TrySetFromTask(Task.CompletedTask));
                tcs.SetFromTask(Task.CompletedTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine(tcs.Task.Status);
                Console.WriteLine("Exception:");
                Console.WriteLine(ex);
            }
            
            Console.WriteLine("Completed");
        }
    }

    // https://github.com/dotnet/runtime/issues/61959
    // https://github.com/dotnet/runtime/pull/100316
    public static async Task WhenEachTest()
    {
        var startTimestamp = TimeProvider.System.GetTimestamp();
        var tasks = Enumerable.Range(0, 5)
            .Select(i => Task.Delay(TimeSpan.FromSeconds(i + 1)))
            ;
        await foreach (var item in Task.WhenEach(tasks))
        {
            Console.WriteLine(item.IsCompletedSuccessfully);
            Console.WriteLine(TimeProvider.System.GetLocalNow());
        }
        var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);
        Console.WriteLine(elapsedTime);
    }
}
