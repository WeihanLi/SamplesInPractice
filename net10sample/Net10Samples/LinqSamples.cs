using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Channels;

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
            JobName = j?.Name ?? "Unknown"
        }))
        {
            Console.WriteLine(JsonSerializer.Serialize(item));
        }
    }

    public static async Task EFLeftJoinSample()
    {
        var services = new ServiceCollection();
        services.AddSqlite<TestDbContext>("Data Source=test.db", optionsAction: options =>
        {
            options.LogTo(Console.WriteLine);
        });
        await using var serviceProvider = services.BuildServiceProvider();
        
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        
        dbContext.Jobs.Add(new Job() { Id = 1, Name = "test" });
        dbContext.Employees.Add(new Employee() { Id = 1, JobId = 1, Name = "Alice" });
        dbContext.Employees.Add(new Employee() { Id = 2, JobId = 2, Name = "Jane" });
        await dbContext.SaveChangesAsync();

        var result = await dbContext.Employees.AsNoTracking()
            // ReSharper disable once EntityFramework.UnsupportedServerSideFunctionCall
            .LeftJoin(dbContext.Jobs.AsNoTracking(), e => e.JobId, j => j.Id, (e, j) => new
            {
                e.Id,
                e.Name,
                e.JobId,
                JobName =  j == null ? "Unknown" : j.Name
            })
            .ToArrayAsync();
        foreach (var item in result)
        {
            Console.WriteLine(JsonSerializer.Serialize(item));
        }
        
        result = await dbContext.Jobs.AsNoTracking()
            // ReSharper disable once EntityFramework.UnsupportedServerSideFunctionCall
            .RightJoin(dbContext.Employees.AsNoTracking(), j => j.Id, e => e.JobId, (j, e) => new
            {
                e.Id,
                e.Name,
                e.JobId,
                JobName =  j == null ? "Unknown" : j.Name
            })
            .ToArrayAsync();
        foreach (var item in result)
        {
            Console.WriteLine(JsonSerializer.Serialize(item));
        }
    }

    public static void ShuffleSamples()
    {
        var source = Enumerable.Range(1, 5).ToArray();
        Console.WriteLine(string.Join(",", source));
        source.Shuffle();
        Console.WriteLine(string.Join(",", source));
        Console.WriteLine(string.Join(",", source.Shuffle()));
    }
}

class Job
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int JobId { get; set; }
}

class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public virtual DbSet<Job> Jobs { get; set; } = null!;
    public virtual DbSet<Employee> Employees { get; set; } = null!;
}
