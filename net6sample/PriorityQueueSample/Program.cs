using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Logging;
using WeihanLi.Extensions;

LogHelper.ConfigureLogging(builder => builder.AddConsole());

//InvokeHelper.TryInvoke(() =>
//{
//    var queue = new PriorityQueue<string, int>();
//    Enumerable.Range(1, 100)
//    .Select(x => Task.Run(() => queue.Enqueue(x.ToString(), x)))
//    .WhenAll().Wait();
//    Console.WriteLine(queue.Count);
//});
//Console.ReadLine();

Sample1();
Console.WriteLine(new string('-', 100));

Sample2();
Console.WriteLine(new string('-', 100));

Sample3();
Console.WriteLine(new string('-', 100));

Console.WriteLine("Hello World!");
Console.ReadLine();

static void Sample1()
{
    var priorityQueue = new PriorityQueue<string, int>();
    priorityQueue.Enqueue("Alice", 100);
    priorityQueue.EnqueueRange(Enumerable.Range(1, 5)
        .Select(x => ($"X_{x}", 100 - x))
    );

    while (priorityQueue.TryDequeue(out var element, out var priority))
    {
        Console.WriteLine($"Element:{element}, {priority}");
    }
}

static void Sample2()
{
    var list = new List<(string name, int score)>
    {
        ("Mike", 99),
        ("Ming", 100),
        ("Jane", 96),
        ("Yi", 94),
        ("Harry", 90),
    };

    var priorityQueue = new PriorityQueue<string, int>();
    priorityQueue.EnqueueRange(list);

    Console.WriteLine("--Unordered:");
    foreach (var item in priorityQueue.UnorderedItems)
    {
        Console.WriteLine($"Name:{item.Element}, score:{item.Priority}");
    }

    Console.WriteLine("--Ordered:");
    while (priorityQueue.TryDequeue(out var name, out var score))
    {
        Console.WriteLine($"Name:{name}, score:{score}");
    }

    Console.WriteLine("-----TOP 3");
    priorityQueue = new PriorityQueue<string, int>(new High2LowComparer());
    priorityQueue.EnqueueRange(list);
    var rank = 0;
    while (rank++ < 3 && priorityQueue.TryDequeue(out var name, out var score))
    {
        Console.WriteLine($"Rank({rank}):Name:{name}, score:{score}");
    }
}

static void Sample3()
{
    var random = new Random();

    var queue = new PriorityQueue<string, (DateTime time, int priority)>(new DateTimePriorityComparer());

    var time = DateTime.UtcNow;
    Thread.Sleep(1000);
    for (var k = 0; k < 3; k++)
    {
        for (var i = 1; i <= 3; i++)
        {
            queue.Enqueue($"Message_{i}_{k}", (i > 2 ? time : DateTime.UtcNow, random.Next(5, 10)));
        }
    }

    while (queue.TryDequeue(out var message, out var priority))
    {
        Console.WriteLine($"{message}, {priority.priority}, {priority.time}");
    }
}

internal class High2LowComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        return y.CompareTo(x);
    }
}

internal class DateTimePriorityComparer : IComparer<(DateTime time, int priority)>
{
    public int Compare((DateTime time, int priority) x, (DateTime time, int priority) y)
    {
        var priorityComparison = x.priority.CompareTo(y.priority);
        if (priorityComparison != 0) return priorityComparison;
        return x.time.CompareTo(y.time);
    }
}
