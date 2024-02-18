// https://github.com/dotnet/runtime/issues/44871
// https://github.com/dotnet/runtime/issues/93925
// https://github.com/dotnet/runtime/pull/93994

namespace Net9Samples;

public static class PriorityQueueSample
{
    public static void MainTest()
    {
        var queue = new PriorityQueue<string, int>();
        
        queue.Enqueue("Lily", 88);
        queue.Enqueue("Ming", 80);
        queue.Enqueue("Alice", 90);
        queue.Enqueue("Mike", 85);
        queue.Enqueue("Amy", 99);
        queue.Enqueue("Alex", 98);

        if (!queue.Remove("Jane", out _, out _))
        {
            Console.WriteLine("Jane not existed");
        }
        
        if (queue.Remove("Alice", out _, out _))
        {
            Console.WriteLine("Alice removed");
        }

        // remove with custom equality comparer
        if (queue.Remove("mike", out var removedElement, out var removedPriority, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"RemovedElement - {removedElement}: {removedPriority}");
        }

        // update priority
        if (queue.Remove("alex", out removedElement, out removedPriority, StringComparer.OrdinalIgnoreCase))
        {
            queue.Enqueue(removedElement, 60);
            Console.WriteLine($"Element priority updated: {removedElement}, previous priority: {removedPriority}, new priority: {60}");
        }

        Console.WriteLine();
        Console.WriteLine("PriorityQueue elements:");
        while (queue.TryDequeue(out var element, out var priority))
        {
            Console.WriteLine($"{element}: {priority}");
        }
        
        Console.WriteLine();
    }
}
