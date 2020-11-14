using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Extensions;

namespace EF5Samples
{
    internal class SaveChangesInterceptorTest
    {
        public static void MainTest()
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseInMemoryDatabase("Tests")
                    //.LogTo(Console.WriteLine)
                    .AddInterceptors(new AuditInterceptor())
                    ;
            });
            using var provider = services.BuildServiceProvider();
            using (var scope = provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
                dbContext.Posts.Add(new Post() { Id = 1, Author = "test", Title = "test", PostedAt = DateTime.UtcNow });
                dbContext.SaveChanges();

                var post = dbContext.Posts.Find(1);
                post.Author = "test2";
                dbContext.SaveChanges();

                dbContext.Posts.Remove(post);
                dbContext.SaveChanges();
            }
        }
    }

    public class AuditInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var changesList = new List<CompareModel>();

            foreach (var entry in
                eventData.Context.ChangeTracker.Entries<Post>())
            {
                if (entry.State == EntityState.Added)
                {
                    changesList.Add(new CompareModel()
                    {
                        OriginalValue = null,
                        NewValue = entry.CurrentValues.ToObject(),
                    });
                }
                else if (entry.State == EntityState.Deleted)
                {
                    changesList.Add(new CompareModel()
                    {
                        OriginalValue = entry.OriginalValues.ToObject(),
                        NewValue = null,
                    });
                }
                else if (entry.State == EntityState.Modified)
                {
                    changesList.Add(new CompareModel()
                    {
                        OriginalValue = entry.OriginalValues.ToObject(),
                        NewValue = entry.CurrentValues.ToObject(),
                    });
                }
                Console.WriteLine($"change list:{changesList.ToJson()}");
            }
            return base.SavingChanges(eventData, result);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            Console.WriteLine($"changes:{eventData.EntitiesSavedCount}");
            return base.SavedChanges(eventData, result);
        }

        private class CompareModel
        {
            public object OriginalValue { get; set; }

            public object NewValue { get; set; }
        }
    }
}
