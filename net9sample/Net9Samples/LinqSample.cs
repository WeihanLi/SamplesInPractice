using System.Text.Json;

namespace Net9Samples;

public static class LinqSample
{
    public static void MainTest()
    {
        var employees = Enumerable.Range(1, 10)
            .Select(x => new { Id = x, Level = x % 4, Name = $"xxx {x}", Score = x * 10 })
            .ToArray();

        // https://github.com/dotnet/runtime/issues/95563
        // https://github.com/dotnet/runtime/pull/95947
        // Index
        Console.WriteLine("Index sample:");
        foreach (var (index, item) in employees.Index())
        {
            Console.WriteLine($"Index: {index}, Id: {item.Id}, {JsonSerializer.Serialize(item)}");
        }

        // https://github.com/dotnet/runtime/issues/77716
        // https://github.com/dotnet/runtime/pull/91507
        // CountBy
        Console.WriteLine("CountBy sample:");
        foreach (var (key, count) in employees.CountBy(x => x.Level))
        {
            Console.WriteLine($"Level {key}, Count: {count}");
        }

        // https://github.com/dotnet/runtime/issues/91533
        // https://github.com/dotnet/runtime/pull/92089
        // AggregateBy
        Console.WriteLine("AggregateBy sample:");
        foreach (var (key, total) in employees.AggregateBy(s => s.Level, 0, (a, x) => a + x.Score))
        {
            Console.WriteLine($"Level {key}: Total: {total}");
        }
        Console.WriteLine();
        foreach (var (key, total) in employees.AggregateBy(s => s.Level, x => x * 10, (a, x) => a + x.Score))
        {
            Console.WriteLine($"Level {key}: Total: {total}");
        }
    }
}
