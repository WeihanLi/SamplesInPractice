using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

var watch = Stopwatch.StartNew();
await Task.WhenAll(Enumerable.Range(1, 100).Select(_ => Task.Delay(1000)));
watch.Stop();
WriteLine(watch.ElapsedMilliseconds);

watch.Restart();
using var semaphore = new SemaphoreSlim(10, 10);
await Task.WhenAll(Enumerable.Range(1, 100).Select(async _ =>
{
    try
    {
        await semaphore.WaitAsync();
        await Task.Delay(1000);
    }
    finally
    {
        semaphore.Release();
    }
}));
watch.Stop();
WriteLine(watch.ElapsedMilliseconds);

WriteLine($"{nameof(Environment.ProcessorCount)}: {Environment.ProcessorCount}");

watch.Restart();
await Parallel.ForEachAsync(Enumerable.Range(1, 100), async (_, _) => await Task.Delay(1000));
watch.Stop();
WriteLine(watch.ElapsedMilliseconds);

watch.Restart();
await Parallel.ForEachAsync(Enumerable.Range(1, 100), new ParallelOptions()
{
    MaxDegreeOfParallelism = 10
}, async (_, _) => await Task.Delay(1000));
watch.Stop();
WriteLine(watch.ElapsedMilliseconds);

watch.Restart();
await Parallel.ForEachAsync(Enumerable.Range(1, 100), new ParallelOptions()
{
    MaxDegreeOfParallelism = 100
}, async (_, _) => await Task.Delay(1000));
watch.Stop();
WriteLine(watch.ElapsedMilliseconds);

watch.Restart();
await Parallel.ForEachAsync(Enumerable.Range(1, 100), new ParallelOptions()
{
    MaxDegreeOfParallelism = int.MaxValue
}, async (_, _) => await Task.Delay(1000));
watch.Stop();
WriteLine(watch.ElapsedMilliseconds);

WriteLine("Hello world");
