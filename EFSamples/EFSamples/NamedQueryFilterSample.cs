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
                .AddDbContext<BlogPostContext>((provider, options) =>
                {
                    options.UseSqlite(connString);
                })
                .BuildServiceProvider()
            ;

        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BlogPostContext>();
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

file sealed class QueryFilterContext : DbContext
{
    public DbSet<BlogPost> Posts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>()
            .HasQueryFilter(p => !p.Title.StartsWith("[Deleted]")) 
            .HasQueryFilter("non-disabled", p => !p.Title.StartsWith("[Disabled]"))
            ;
        base.OnModelCreating(modelBuilder);
    }
}
