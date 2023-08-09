using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;
using WeihanLi.Common;
using WeihanLi.Common.Services;

namespace Net8Sample;

public static class KeyedServiceSample
{
    public static void MainTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IUserIdProvider, EnvironmentUserIdProvider>("env");
        serviceCollection.AddKeyedSingleton<IUserIdProvider, EnvironmentUserIdProvider>(null);

        using var services = serviceCollection.BuildServiceProvider();
        var userIdProvider = services.GetRequiredKeyedService<IUserIdProvider>(null);
        Console.WriteLine(userIdProvider.GetUserId());

        var envUserIdProvider = services.GetRequiredKeyedService<IUserIdProvider>("env");
        Console.WriteLine(envUserIdProvider.GetUserId());

        Console.WriteLine("userIdProvider == envUserIdProvider? {0}", envUserIdProvider == userIdProvider);
    }
}

