namespace EF6Samples;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options):base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
}

public record User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; init; }
}
