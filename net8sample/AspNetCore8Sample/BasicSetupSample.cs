namespace AspNetCore8Sample;

public class BasicSetupSample
{
    public static async Task MainTest(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddControllers();
        var app = builder.Build();
        app.MapControllers();
        await app.RunAsync();
    }
}
