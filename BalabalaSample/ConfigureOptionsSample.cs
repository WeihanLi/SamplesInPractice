using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BalabalaSample;

public class ConfigureOptionsSample
{
    public static void MainTest()
    {
        var services = new ServiceCollection();
        
        services.Configure<NumberOption>(null,o =>
        {
            o.Num = 2;
        });
        
        
        services.Configure<NumberOption>(o =>
        {
            o.Num++;
        });
        
        services.Configure<NumberOption>("h", o =>
        {
            o.Num++;
        });
        using var sp = services.BuildServiceProvider();
        
        Console.WriteLine(sp.GetRequiredService<IOptionsMonitor<NumberOption>>()
            .Get("h").Num);
    }
}


file sealed class NumberOption
{
    public int Num { get; set; }
}
