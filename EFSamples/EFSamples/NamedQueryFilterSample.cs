using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFSamples;

public class NamedQueryFilterSample
{
    public static async Task Run()
    {
        const string connString = "DataSource=QueryFilterSample.db";
        await using var services = new ServiceCollection()
                .AddLogging(lb => lb.AddDefaultDelegateLogger())
                .AddDbContext<QueryFilterContext>((provider, options) =>
                {
                    options.UseSqlite(connString);
                })
                .BuildServiceProvider()
            ;

        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<QueryFilterContext>();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            {
                context.Posts.Add(new BlogPost
                {
                    Title = "test",
                    UpdatedAt = DateTimeOffset.Now,
                    UpdatedBy = "test"
                });
                context.Posts.Add(new BlogPost 
                { 
                    Title = "[Disabled]test",
                    UpdatedAt = DateTimeOffset.Now,
                    UpdatedBy = "test" 
                });
                context.Posts.Add(new BlogPost
                {
                    Title = "[Deleted]test",
                    UpdatedAt = DateTimeOffset.Now,
                    UpdatedBy = "test"
                });
                await context.SaveChangesAsync();
            }

            {
                var posts = await context.Posts.AsNoTracking()
                    .IgnoreQueryFilters().ToArrayAsync();
                Console.WriteLine(posts.Length);
                Console.WriteLine();
            }

            {
                var posts = await context.Posts.AsNoTracking().ToArrayAsync();
                Console.WriteLine(posts.Length);
                Console.WriteLine();
            }
            
            {
                var posts = await context.Posts.AsNoTracking()
                    .IgnoreQueryFilters(["non-deleted"]).ToArrayAsync();
                Console.WriteLine(posts.Length);
                Console.WriteLine();
            }
        }
    }
}

file sealed class QueryFilterContext(DbContextOptions<QueryFilterContext> options) : DbContext(options)
{
    public DbSet<BlogPost> Posts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>()
            .HasQueryFilter("non-deleted", p => !p.Title.StartsWith("[Deleted]")) 
            .HasQueryFilter("non-disabled", p => !p.Title.StartsWith("[Disabled]"))
            ;
        base.OnModelCreating(modelBuilder);
    }
}
