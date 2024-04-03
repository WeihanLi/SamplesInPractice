// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using WeihanLi.Common.Helpers;

{
    Console.WriteLine("MyThreadPool sample");
    MyThreadPool.QueueWorkItem(() =>
    {
        Console.WriteLine("Hello World");
        Console.WriteLine($"Thread: {Thread.CurrentThread.Name} {Thread.CurrentThread.IsBackground}");
    });
    ConsoleHelper.ReadLineWithPrompt();
}

{
    Console.WriteLine("MyTask.Run sample");
    
    MyTask.Run(delegate
    {
        Console.WriteLine("Hello World");
        Console.WriteLine($"Thread: {Thread.CurrentThread.Name} BackgroundThread: {Thread.CurrentThread.IsBackground}");
    });
    ConsoleHelper.ReadLineWithPrompt();
    
    Console.WriteLine("MyTask.Run/Wait sample");

    for (var i = 0; i < 5; i++)
    {
        var id = i;
        MyTask.Run(delegate
        {
            Thread.Sleep(1000);
            Console.WriteLine(id);
        }).Wait();
    }
}

{
    Console.WriteLine("MyTask.WhenAll sample");
    var tasks = new List<MyTask>();
    for (var i = 0; i < 5; i++)
    {
        var v = i;
        var t = MyTask.Run(() =>
        {
            Thread.Sleep(v * 100);
            Console.WriteLine(v);
        });
        tasks.Add(t);
    }
    MyTask.WhenAll(tasks).ContinueWith(()=> Console.WriteLine("MyTask.WhenAll completed")).Wait();
}

{
    Console.WriteLine("MyTask.WhenAny sample");
    var tasks = new List<MyTask>();
    for (var i = 0; i < 5; i++)
    {
        var v = i;
        var t = MyTask.Run(() =>
        {
            Thread.Sleep(v * 100);
            Console.WriteLine(v);
        });
        tasks.Add(t);
    }
    MyTask.WhenAny(tasks).ContinueWith(()=> Console.WriteLine("MyTask.WhenAny completed")).Wait();
}

{
    Console.WriteLine("MyTask.Delay sample");
    Console.WriteLine(TimeProvider.System.GetLocalNow());
    MyTask.Delay(TimeSpan.FromSeconds(2)).Wait();
    Console.WriteLine($"Delay worked? {TimeProvider.System.GetLocalNow()}");
}

{
    Console.WriteLine("MyTask.Iterate sample");
    MyTask.Iterate(PrintAsync()).Wait();
}

{
    Console.WriteLine("await MyTask sample");
    Console.WriteLine(TimeProvider.System.GetLocalNow());
    Console.WriteLine("Hello");
    await MyTask.Run(() =>
    {
        Thread.Sleep(1000);
        Console.WriteLine("World");
    });
    Console.WriteLine(TimeProvider.System.GetLocalNow());
}

Console.WriteLine("Completed!");
Console.ReadLine();


static IEnumerable<MyTask> PrintAsync()
{
    for (var i = 0; i < 5; i++)
    {
        yield return MyTask.Delay(1000);
        Console.WriteLine(i);
    }
}

public sealed class MyTask
{
    private bool _completed;
    private Exception? _exception;
    private Action? _continuation;
    private ExecutionContext? _executionContext;
    private readonly object _lock = new();

    public bool IsCompleted
    {
        get
        {
            lock (_lock)
            {
                return _completed;
            }
        }
    }

    public void SetException(Exception exception)
    {
        Complete(exception);
    }

    public void SetResult()
    {
        Complete();
    }
    
    private void Complete(Exception? exception = null)
    {
        lock (_lock)
        {
            if (_completed)
            {
                throw new InvalidOperationException("Task has been completed, can not set result more than once");
            }

            _completed = true;
            _exception = exception;

            if (_continuation is not null)
            {
                MyThreadPool.QueueWorkItem(delegate
                {
                    if (_executionContext is null)
                    {
                        _continuation();
                    }
                    else
                    {
                        ExecutionContext.Run(_executionContext, state => ((Action)state!)(), _continuation);
                    }
                });
            }
        }
    }

    public MyTask ContinueWith(Action continuation)
    {
        var task = new MyTask();
        var callback = () =>
        {
            try
            {
                continuation();
                task.SetResult();
            }
            catch (Exception e)
            {
                task.SetException(e);
            }
        };
        
        lock (_lock)
        {
            if (_completed)
            {
                MyThreadPool.QueueWorkItem(callback);
            }
            else
            {
                _continuation = callback;
                _executionContext = ExecutionContext.Capture();
            }   
        }

        return task;
    }

    public void Wait()
    {
        ManualResetEventSlim? resetEvent = null;
        
        lock (_lock)
        {
            if (!_completed)
            {
                resetEvent = new ManualResetEventSlim();
                ContinueWith(resetEvent.Set);
            }
        }
        
        resetEvent?.Wait();
        
        if (_exception is not null)
        {
            // throw new AggregateException(_exception);
            ExceptionDispatchInfo.Throw(_exception);
        }
    }
    
    public MyTaskAwaiter GetAwaiter()
    {
        return new MyTaskAwaiter(this);
    }

    public static MyTask Delay(long ms) => Delay(TimeSpan.FromMilliseconds(ms));
    public static MyTask Delay(TimeSpan delay)
    {
        var task = new MyTask();
        new Timer(_ => { task.SetResult(); })
            .Change(delay, Timeout.InfiniteTimeSpan);
        return task;
    }

    public static MyTask WhenAll(List<MyTask> tasks)
    {
        var whenAllTask = new MyTask();
        if (tasks is { Count: > 0 })
        {
            var remainingTaskCount = tasks.Count; 
            var continuation = () =>
            {
                var remainingCount = Interlocked.Decrement(ref remainingTaskCount);
                if (remainingCount is 0)
                {
                    whenAllTask.SetResult();
                }
            };
            foreach (var task in tasks)
            {
                task.ContinueWith(continuation);
            }
        }
        else
        {
            whenAllTask.SetResult();   
        }
        return whenAllTask;
    }
    
    public static MyTask WhenAny(List<MyTask> tasks)
    {
        var whenAnyTask = new MyTask();
        if (tasks is { Count: > 0 })
        {
            var completedTaskCount = 0; 
            var continuation = () =>
            {
                var remainingCount = Interlocked.Increment(ref completedTaskCount);
                if (remainingCount == 1)
                {
                    whenAnyTask.SetResult();
                }
            };
            foreach (var task in tasks)
            {
                task.ContinueWith(continuation);
            }
        }
        else
        {
            whenAnyTask.SetResult();   
        }
        return whenAnyTask;
    }
    
    public static MyTask Iterate(IEnumerable<MyTask> tasks)
    {
        var t = new MyTask();

        using var enumerator = tasks.GetEnumerator();

        MoveNext();
        return t;

        void MoveNext()
        {
            try
            {
                if (enumerator.MoveNext())
                {
                    var next = enumerator.Current;
                    next.ContinueWith(MoveNext);
                    return;
                }
            }
            catch (Exception e)
            {
                t.SetException(e);
            }
            
            t.SetResult();
        }
    }

    public static MyTask Run(Action action)
    {
        var task = new MyTask();
        MyThreadPool.QueueWorkItem(() =>
        {
            try
            {
                action();
                task.SetResult();
            }
            catch (Exception e)
            {
                task.SetException(e);
            }
        });
        return task;
    }

}

public sealed class MyTaskAwaiter(MyTask myTask) : INotifyCompletion
{
    public void GetResult()
    {
        myTask.Wait();
    }
    public bool IsCompleted => myTask.IsCompleted;
    public void OnCompleted(Action continuation)
    {
        myTask.ContinueWith(continuation);
    }
}

file static class MyThreadPool
{
    private static readonly BlockingCollection<WorkItem> WorkItems = new();
    private static readonly Thread[] Threads = new Thread[Environment.ProcessorCount];

    static MyThreadPool()
    {
        for (var i = 0; i < Threads.Length; i++)
        {
            Threads[i] = new Thread(() =>
            {
                while (true)
                {
                    if (WorkItems.TryTake(out var workItem))
                    {
                        Debug.WriteLine("WorkItem taken from queue");
                        
                        if (workItem.ExecutionContext is null)
                        {
                            workItem.Action();
                        }
                        else
                        {
                            ExecutionContext.Run(workItem.ExecutionContext, (object? obj) => ((Action)obj!)(),
                                workItem.Action);
                        }

                        Debug.WriteLine("WorkItem handled");
                    }
                }
            }) { IsBackground = true, Name = $"{nameof(MyThreadPool)}-WorkerThread-{i}" };
            Threads[i].Start();
        }
    }

    public static void QueueWorkItem(Action action)
    {
        WorkItems.Add(new WorkItem() { Action = action, ExecutionContext = ExecutionContext.Capture() });
        Debug.WriteLine("WorkItem queued");
    }
}

file sealed class WorkItem
{
    public required Action Action { get; set; }
    public ExecutionContext? ExecutionContext { get; set; }
}
