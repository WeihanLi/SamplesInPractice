using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EF5Samples
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public virtual DbSet<Post> Posts { get; set; }
    }

    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string Author { get; set; }

        public string Title { get; set; }

        public DateTime PostedAt { get; set; }
    }
}
