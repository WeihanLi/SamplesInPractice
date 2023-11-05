using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Services;

namespace BalabalaSample;

public static class DependencyInjectionSample
{
    public static async Task MainTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IIdGenerator>(sp => null!);
        await using var serviceProvider = services.BuildServiceProvider();
        
        foreach (var generator in serviceProvider.GetServices<IIdGenerator>())
        {
            InvokeHelper.TryInvoke(() => Console.WriteLine(generator.NewId()));
        }
    }
}
