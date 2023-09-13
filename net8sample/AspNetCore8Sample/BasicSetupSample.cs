namespace AspNetCore8Sample;

public class BasicSetupSample
{
    public static async Task MainTest()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddControllers();
        var app = builder.Build();
        app.MapControllers();
        await app.RunAsync();
    }
}
