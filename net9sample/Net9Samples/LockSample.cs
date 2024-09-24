using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Net9Samples;

public static class LockSample
{
    public static void MainTest()
    {
        Console.WriteLine(Enumerable.Range(1, 1000).Sum());
        Console.WriteLine();

        {
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                sum += i;
            });

            Console.WriteLine(sum);
        }

        {
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                lock (nameof(MainTest))
                {
                    sum += i;
                }
            });

            Console.WriteLine(sum);
        }


        {
            var locker = new object();
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                lock (locker)
                {
                    sum += i;
                }
            });

            Console.WriteLine(sum);
        }

        {
            var locker = new Lock();
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                lock (locker)
                {
                    sum += i;
                }
            });

            Console.WriteLine(sum);
        }

        {
            var locker = new Lock();
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                using (locker.EnterScope())
                {
                    sum += i;
                }
            });

            Console.WriteLine(sum);
        }

    }

    public static void LockObjectPerfTest()
    {
        BenchmarkRunner.Run<LockObjectBenchmark>();
    }
}

[SimpleJob]
[MemoryDiagnoser]
public class LockObjectBenchmark
{
    private readonly object _lock0 = new();
    private readonly Lock _lock1 = new();

    [Benchmark(Baseline = true)]
    public int NewLockObject()
    {
        var i = 0;
        Parallel.For(1, 1000, _ =>
        {
            lock (_lock1)
            {
                Interlocked.Increment(ref i);
            }
        });
        return i;
    }

    [Benchmark]
    public int TraditionalLock()
    {
        var i = 0;
        Parallel.For(1, 1000, _ =>
        {
            lock (_lock0)
            {
                Interlocked.Increment(ref i);
            }
        });
        return i;
    }
}
