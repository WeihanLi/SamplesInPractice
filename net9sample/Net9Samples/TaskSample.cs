namespace Net9Samples;

// https://github.com/dotnet/runtime/issues/47998
// https://github.com/dotnet/runtime/pull/97077
public class TaskSample
{
    public static async Task MainTest()
    {
        {
            var tcs = new TaskCompletionSource();

            _ = Task.Delay(2000).ContinueWith(r => tcs.SetFromTask(r));

            await tcs.Task;
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
                Console.WriteLine("Exception:");
                Console.WriteLine(ex);
            }
            
            Console.WriteLine("Completed");
        }
        
        
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetFromTask(Task.FromResult(100));
            Console.WriteLine(await tcs.Task);
            Console.WriteLine("Completed");
        }
    }
}
