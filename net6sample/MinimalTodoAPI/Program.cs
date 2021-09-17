var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<TodoDbContext>(builder.Configuration.GetConnectionString("Todo"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MinimalTodoAPI", Version = "v1" });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<TodoDbContext>()
        .Database.EnsureCreatedAsync();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MinimalTodoAPI v1"));
}

app.Map("/health", Results.Ok);
app.MapGet("/contextSample", (HttpContext context) =>
 {
     return Results.Ok(context.Request.Query);
 }).ExcludeFromDescription();

app.MapGet("/api/todo", (TodoDbContext dbContext) => dbContext.TodoItems.AsNoTracking().ToArrayAsync());
app.MapPost("/api/todo", async (TodoItem item, TodoDbContext dbContext) => 
{
    if(string.IsNullOrWhiteSpace(item?.Title))
    {
        return Results.BadRequest();
    }
    item.Id = 0;
    item.CreatedAt = DateTime.UtcNow;
    dbContext.TodoItems.Add(item);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/api/todo/{item.Id}", item);
});
app.MapPut("/api/todo/{id}", async (int id, TodoItem item, TodoDbContext dbContext) => 
{
    if(id <= 0)
    {
        return Results.BadRequest();
    }
    var todo = await dbContext.TodoItems.FindAsync(id);
    if(todo is null)
    {
        return Results.NotFound();
    }
    todo.Title = item.Title;
    todo.Description = item.Description;
    todo.Done = item.Done;
    await dbContext.SaveChangesAsync();
    return Results.Ok(todo);
});
app.MapDelete("/api/todo/{id}", async (int id, TodoDbContext dbContext) =>
{
    if (id <= 0)
    {
        return Results.BadRequest();
    }
    var todo = await dbContext.TodoItems.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }
    dbContext.Remove(todo);
    await dbContext.SaveChangesAsync();
    return Results.Ok(todo);
});

app.Run();
