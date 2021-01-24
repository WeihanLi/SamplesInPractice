using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EF5Samples
{
    public class SavePointsTest
    {
        public static void MainTest()
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseSqlite("Data Source=Application.db;Cache=Shared")
                    .LogTo(Console.WriteLine, LogLevel.Warning)
                    ;
            });
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            dbContext.Database.EnsureCreated();
            Console.WriteLine($"Posts count:{dbContext.Posts.Count()}");
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                dbContext.Posts.Add(new Post() { Author = "Tom", Title = "Date changed", PostedAt = DateTime.UtcNow, });
                dbContext.Posts.Add(new Post() { Author = "Tom", Title = "Date changed", PostedAt = DateTime.UtcNow, });
                dbContext.SaveChanges();
                transaction.CreateSavepoint("Stage1");
                Console.WriteLine($"Posts count:{dbContext.Posts.Count()}");

                dbContext.Posts.Add(new Post() { Author = "Alice", Title = "Test", PostedAt = DateTime.UtcNow, });
                dbContext.SaveChanges();
                transaction.CreateSavepoint("Stage2");
                Console.WriteLine($"Posts count:{dbContext.Posts.Count()}");

                throw new InvalidOperationException();

                // transaction.Commit();
            }
            catch (Exception)
            {
                Console.WriteLine("Exception throw");
                transaction.RollbackToSavepoint("Stage1");
            }

            Console.WriteLine($"Posts count:{dbContext.Posts.Count()}");
        }
    }
}
