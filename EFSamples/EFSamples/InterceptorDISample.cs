﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using WeihanLi.Common.Services;

namespace EFSamples;

public static class InterceptorDISample
{
    public static async Task MainTest()
    {
        const string sqlLiteConnectionString = "DataSource=Blog";

        await using var services = new ServiceCollection()
                .AddLogging(lb => lb.AddDelegateLogger((category, level, exception, msg) =>
                {
                    Console.WriteLine($"[{level}][{category}] {msg}\n{exception}");
                }))
                .AddSingleton<IUserIdProvider, UserIdProvider>()
                .AddSingleton<DIAutoUpdateInterceptor>()
                .AddDbContext<BlogPostContext>((provider, options) =>
                {
                    options.AddInterceptors(provider.GetRequiredService<DIAutoUpdateInterceptor>());
                    options.UseSqlite(sqlLiteConnectionString);
                })
                .BuildServiceProvider()
            ;
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogPostContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        
        dbContext.Posts.Add(new BlogPost()
        {
            Title = "test",
        });
        await dbContext.SaveChangesAsync();

        var posts = await dbContext.Posts.AsNoTracking().ToArrayAsync();
        Console.WriteLine(JsonSerializer.Serialize(posts));
    }
}

file sealed class UserIdProvider : IUserIdProvider
{
    public string GetUserId() => "Admin";
}

file sealed class BlogPostContext(DbContextOptions<BlogPostContext> options): DbContext(options)
{
    public DbSet<BlogPost> Posts { get; set; } = default!;
}

public class BlogPost
{
    public int Id { get; set; }

    [StringLength(64)]
    public required string Title { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    [StringLength(64)]
    public string UpdatedBy { get; set; } = default!;
}

file sealed class DIAutoUpdateInterceptor(IUserIdProvider userIdProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        ArgumentNullException.ThrowIfNull(eventData.Context);
        foreach (var entry in eventData.Context.ChangeTracker.Entries<BlogPost>())
        {
            if (entry.State is not EntityState.Added) continue;
            
            entry.Entity.UpdatedAt = DateTimeOffset.Now;
            entry.Entity.UpdatedBy = userIdProvider.GetUserId() ?? "";
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
