const string connectionString = "DataSource=test";

var services = new ServiceCollection();

// services.AddDbContext<TestDbContext>(options => options.UseSqlite(connectionString));
services.AddSqlite<TestDbContext>(connectionString);

using var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
dbContext.Database.EnsureDeleted();
dbContext.Database.EnsureCreated();
dbContext.Users.Add(new User 
{ 
    Name = "Alice",
    CreatedAt = DateTime.UtcNow
});
await dbContext.SaveChangesAsync();

var users = await dbContext.Users.AsNoTracking().ToArrayAsync();
users.Dump();
