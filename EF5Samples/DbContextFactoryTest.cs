using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Extensions;

namespace EF5Samples
{
    public class DbContextFactoryTest
    {
        public static void MainTest()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<TestDbContext>(options =>
            {
                options.UseInMemoryDatabase("Tests")
                    ;
            });
            using var provider = services.BuildServiceProvider();
            var contextFactory = provider.GetRequiredService<IDbContextFactory<TestDbContext>>();
            using (var dbContext = contextFactory.CreateDbContext())
            {
                Enumerable.Range(1, 100)
                    .Select(i =>
                   {
                       dbContext.Posts.Add(new Post() { Id = i + 1, Author = $"author_{i}", Title = $"title_{i}" });
                       return dbContext.SaveChangesAsync();
                   })
                    .WhenAll()
                    .Wait();

                Console.WriteLine(dbContext.Posts.Count());
            }

            Enumerable.Range(1, 100)
                .Select(async i =>
                {
                    using (var dbContext = contextFactory.CreateDbContext())
                    {
                        dbContext.Posts.Add(new Post() { Id = i + 101, Author = $"author_{i}", Title = $"title_{i}" });
                        return await dbContext.SaveChangesAsync();
                    }
                })
                .WhenAll()
                .Wait();

            using var context = contextFactory.CreateDbContext();
            Console.WriteLine(context.Posts.Count());
        }
    }
}
