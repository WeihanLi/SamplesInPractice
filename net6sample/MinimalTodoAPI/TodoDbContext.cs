namespace MinimalTodoAPI;    
public class TodoDbContext:DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> contextOptions):base(contextOptions)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; } = null!;
}
