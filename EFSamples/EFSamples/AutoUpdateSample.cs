using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using WeihanLi.Common.Models;

namespace EFSamples;

public static class AutoUpdateSample
{
    public static async Task MainTest()
    {
        // const string sqlLiteConnectionString = "DataSource=MyTestDb";
        const string sqlServerConnectionString = "Server=.;Database=MyTestDb;User Id=sa;Password=Test1234;TrustServerCertificate=True;";

        await using var services = new ServiceCollection()
                .AddLogging(lb => lb.AddDelegateLogger((category, level, exception, msg) =>
                {
                    Console.WriteLine($"[{level}][{category}] {msg}\n{exception}");
                }))
                .AddDbContext<TestDbContext>(options =>
                {
                    options.AddInterceptors(new SavingInterceptor());
                    options.UseSqlServer(sqlServerConnectionString);
                })
                .BuildServiceProvider()
            ;
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var job = new Job() { Title = "test" };
        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync();

        await Task.Delay(1000);
        job.Title = "test2";
        await dbContext.SaveChangesAsync();
        
        var jobs = await dbContext.Jobs.AsNoTracking().ToArrayAsync();
        Console.WriteLine(JsonSerializer.Serialize(jobs));
    }
}

file sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs { get; set; } = default!;
}

public sealed class Job : IEntityWithCreatedUpdatedAt
{
    public int Id { get; set; }
    [StringLength(120)]
    public required string Title { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

file sealed class SavingInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData.Context);
        BeforeSaveChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData.Context);
        BeforeSaveChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void BeforeSaveChanges(DbContext dbContext)
    {
        // foreach (var entry in dbContext.ChangeTracker.Entries<Job>())
        // {
        //     var entity = entry.Entity;
        //     
        //     switch (entry.State)
        //     {
        //         case EntityState.Added:
        //             entity.CreatedAt = entity.UpdatedAt = DateTimeOffset.Now;
        //             break;
        //       
        //         case EntityState.Added or EntityState.Modified:
        //             entity.UpdatedAt = DateTimeOffset.Now;
        //             break;
        //     }
        // }
        
        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.Entity is not IEntityWithCreatedUpdatedAt entity) continue;
        
            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = entity.UpdatedAt = DateTimeOffset.Now;
                    break;
              
                case EntityState.Added or EntityState.Modified:
                    entity.UpdatedAt = DateTimeOffset.Now;
                    break;
            }
        }
    }
}
