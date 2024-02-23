namespace Net9Samples;

// https://github.com/dotnet/runtime/issues/47998
// https://github.com/dotnet/runtime/pull/97077
public class TaskSample
{
    public static async Task MainTest()
    {
        {
            var tcs = new TaskCompletionSource();

            _ = Task.Delay(2000).ContinueWith(r=> tcs.SetFromTask(r));

            await tcs.Task;
            Console.WriteLine("Completed");
        }
        
        {
            var tcs = new TaskCompletionSource();
            using var cts = new CancellationTokenSource(200);
            _ = Task.Delay(2000, cts.Token).ContinueWith(r=> tcs.SetFromTask(r));

            await tcs.Task;
            Console.WriteLine("Completed");
        }
    }
}
