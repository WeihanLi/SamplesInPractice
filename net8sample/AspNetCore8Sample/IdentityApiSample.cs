using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore8Sample;

public static class IdentityApiSample
{
    public static async Task MainTest(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        // Swagger/OpenAPI services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSqlite<AppDbContext>("Data Source=identity.db");
        builder.Services.AddIdentityApiEndpoints<AppUser>()
            .AddEntityFrameworkStores<AppDbContext>();
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseAuthorization();
        app.MapGroup("/account").MapIdentityApi<AppUser>();
        app.Map("/", () => "Hello World").RequireAuthorization();
        await app.RunAsync();
    }
}

file sealed class AppUser : IdentityUser
{
}

file sealed class AppDbContext : IdentityUserContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
