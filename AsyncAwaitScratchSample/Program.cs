// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using WeihanLi.Common.Helpers;

// MyThreadPool.QueueWorkItem(() =>
// {
//     Console.WriteLine("Hello World");
//     Console.WriteLine($"Thread: {Thread.CurrentThread.Name} {Thread.CurrentThread.IsBackground}");
// });

MyTask.Run(delegate
{
    Console.WriteLine("Hello World");
    Console.WriteLine($"Thread: {Thread.CurrentThread.Name} BackgroundThread: {Thread.CurrentThread.IsBackground}");
});

Console.WriteLine("Hello!");
Console.ReadLine();


file sealed class MyTask
{
    private bool _completed;
    private Exception? _exception;
    private Action? _continuation;

    public bool IsCompleted
    {
        get
        {
            lock (this)
            {
                return _completed;   
            }
        }
    }

    public void SetResult(Exception? exception = null)
    {
        lock (this)
        {
            if (_completed)
            {
                throw new InvalidOperationException("Task has been completed, can not set result more than once");
            }
            
            _completed = true;
            _exception = exception;

            if (_continuation is not null)
            {
                MyThreadPool.QueueWorkItem(_continuation);
            }
        }
    }

    public void ContinueWith(Action continuation)
    {
        _continuation = continuation;
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
                task.SetResult(e);
            }
        });
        return task;
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
                while (!ApplicationHelper.ExitToken.IsCancellationRequested)
                {
                    if (WorkItems.TryTake(out var workItem))
                    {
                        if(workItem.ExecutionContext is null)
                        {
                            workItem.Action();
                        }
                        else
                        {
                            ExecutionContext.Run(workItem.ExecutionContext, (object? obj) => ((Action)obj!)(), workItem.Action);
                        }
                    }   
                }
            })
            {
                IsBackground = true,
                Name = $"{nameof(MyThreadPool)}-WorkerThread-{i}"
            };
            Threads[i].Start();
        }
    }
    
    public static void QueueWorkItem(Action action)
    {
        WorkItems.Add(new WorkItem()
        {
            Action = action,
            ExecutionContext = ExecutionContext.Capture()
        });
    }
}

file sealed class WorkItem
{
    public required Action Action { get; set; }
    public ExecutionContext? ExecutionContext { get; set; }
}
