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
