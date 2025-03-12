using System.Text.Json;

namespace Net10Samples;

public class LinqSamples
{
    public static async Task AsyncSamples()
    {
        var source = Enumerable.Range(1, 5);
        Console.WriteLine(await AsyncEnumerable.Empty<int>().CountAsync());

        var a = source.ToAsyncEnumerable();
        var array = await a.ToArrayAsync();
        foreach (var item in array)
        {
            Console.WriteLine(item);
        }
    }

    // https://github.com/dotnet/runtime/issues/110292
    // https://github.com/dotnet/runtime/pull/110872
    public static void LeftRightJoinSample()
    {
        var jobs = new[]
        {
            new
            {
                Id = 1,
                Name = "test"
            }
        };
        var employeeList = new[]
        {
            new
            {
                Id = 1,
                JobId = 1,
                Name = "Alice"
            },
            new
            {
                Id = 2,
                JobId = 2,
                Name = "Jane"
            }
        };
        var result = employeeList.LeftJoin(jobs, e => e.JobId, j => j.Id, (e, j) => new
        {
            e.Id,
            e.Name,
            e.JobId,
            JobName = j?.Name ?? "Unnamed"
        });
        foreach (var item in result)
        {
            Console.WriteLine(JsonSerializer.Serialize(item));
        }

        foreach (var item in jobs.RightJoin(employeeList, j => j.Id, e => e.JobId, (j, e) => new
        {
            e.Id,
            e.Name,
            e.JobId,
            JobName = j?.Name ?? "Unnamed"
        }))
        {
            Console.WriteLine(JsonSerializer.Serialize(item));
        }

    }

    public static void ShuffleSamples()
    {
        var source = Enumerable.Range(1, 5);
        // source.Shuffle();
    }
}
