using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Http;
using WeihanLi.Common.Services;

namespace Net7Sample;

public class ServiceCollectionSample
{
    public static async Task MainTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IUserIdProvider, EnvironmentUserIdProvider>();
        await using (var provider = services.BuildServiceProvider())
        {
            Console.WriteLine(provider.GetRequiredService<IUserIdProvider>().GetHashCode());
        }

        await using (var provider2 = services.BuildServiceProvider())
        {
            Console.WriteLine(provider2.GetRequiredService<IUserIdProvider>().GetHashCode());
        }

        //
        Console.WriteLine(services.IsReadOnly);

        // https://github.com/dotnet/runtime/pull/68051
        services.MakeReadOnly();

        Console.WriteLine(services.IsReadOnly);

        try
        {
            services.AddSingleton<IHttpRequester, HttpClientHttpRequester>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
