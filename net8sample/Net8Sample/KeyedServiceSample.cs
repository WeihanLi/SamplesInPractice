using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Otp;
using WeihanLi.Common.Services;

namespace Net8Sample;

public static class KeyedServiceSample
{
    public static void MainTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IUserIdProvider, EnvironmentUserIdProvider>("env");
        serviceCollection.AddKeyedSingleton<IUserIdProvider, EnvironmentUserIdProvider>(null);

        serviceCollection.Configure<TotpOptions>(string.Empty, x =>
        {
            x.Salt = "1234";
        });
        serviceCollection.AddKeyedTransient<ITotpService, TotpService>(string.Empty, (sp, key)=>
            new TotpService(sp.GetRequiredService<IOptionsMonitor<TotpOptions>>().Get(key?.ToString() ?? Options.DefaultName)));
            
        // serviceCollection.AddKeyedService<ITotpService, TotpService, TotpOptions>(string.Empty, options =>
        // {
        //     options.Salt = "1234";
        // });
        
        serviceCollection.AddKeyedScoped<IIdGenerator, GuidIdGenerator>("guid");
        
        using var services = serviceCollection.BuildServiceProvider();
        var userIdProvider = services.GetRequiredKeyedService<IUserIdProvider>(null);
        Console.WriteLine(userIdProvider.GetUserId());
        
        var envUserIdProvider = services.GetRequiredKeyedService<IUserIdProvider>("env");
        Console.WriteLine(envUserIdProvider.GetUserId());
        Console.WriteLine("userIdProvider == envUserIdProvider ?? {0}", userIdProvider == envUserIdProvider);

        var totpService = services.GetRequiredKeyedService<ITotpService>(string.Empty);
        Console.WriteLine(totpService.GetCode(Base36Encoder.Encode(Guid.NewGuid())));
        
        using var scope = services.CreateScope();
        var newId = scope.ServiceProvider.GetRequiredKeyedService<IIdGenerator>("guid").NewId();
        Console.WriteLine(newId);
    }
    
    private static void AddKeyedService<TService, TImplement, TOptions>(this IServiceCollection serviceCollection, string key, Action<TOptions> optionsConfigure)
        where TService : class
        where TImplement: class, TService
        where TOptions: class
    {
        serviceCollection.AddKeyedTransient<TService, TImplement>(key);
        serviceCollection.Configure(key, optionsConfigure);
    }
}

