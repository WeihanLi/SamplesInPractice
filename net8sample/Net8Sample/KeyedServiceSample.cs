using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeihanLi.Common;
using WeihanLi.Common.Models;
using WeihanLi.Common.Otp;
using WeihanLi.Common.Services;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace Net8Sample;

public static class KeyedServiceSample
{
    public static void Sample1()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IUserIdProvider, EnvUserIdProvider>("env");
        serviceCollection.AddKeyedSingleton<IUserIdProvider, NullUserIdProvider>("");

        using var services = serviceCollection.BuildServiceProvider();
        var userIdProvider = services.GetRequiredKeyedService<IUserIdProvider>("");
        Console.WriteLine(userIdProvider.GetUserId());

        var envUserIdProvider = services.GetRequiredKeyedService<IUserIdProvider>("env");
        Console.WriteLine(envUserIdProvider.GetUserId());
    }

    public static void AnyKeySample()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IUserIdProvider, NullUserIdProvider>(KeyedService.AnyKey);

        using var services = serviceCollection.BuildServiceProvider();
        var userIdProvider = services.GetRequiredKeyedService<IUserIdProvider>("");
        Console.WriteLine(userIdProvider.GetUserId());

        var envUserIdProvider = services.GetRequiredKeyedService<IUserIdProvider>("env");
        Console.WriteLine(envUserIdProvider.GetUserId());
        
        Console.WriteLine("userIdProvider == envUserIdProvider ?? {0}", userIdProvider == envUserIdProvider);
        
        var nullUserIdProvider = services.GetRequiredKeyedService<IUserIdProvider>(null);
        Console.WriteLine(nullUserIdProvider.GetUserId());
    }

    public static void ScopedSample()
    {
        var serviceCollection = new ServiceCollection();    
        serviceCollection.AddKeyedScoped<IUserIdProvider, NullUserIdProvider>("");
        using var services = serviceCollection.BuildServiceProvider();
        using var scope = services.CreateScope();
        var newId = scope.ServiceProvider.GetRequiredKeyedService<IIdGenerator>("").NewId();
        Console.WriteLine(newId);
    }

    public static void ServiceKeySample()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<MyNamedService>(KeyedService.AnyKey);
        serviceCollection.AddKeyedTransient<MyKeyedService>(KeyedService.AnyKey);
        using var services = serviceCollection.BuildServiceProvider();
        Console.WriteLine(services.GetRequiredKeyedService<MyNamedService>("Foo").Name);
        Console.WriteLine(services.GetRequiredKeyedService<MyNamedService>("Hello").Name);
        
        // Console.WriteLine(services.GetRequiredKeyedService<MyNamedService>(123).Name);

        Console.WriteLine(services.GetRequiredKeyedService<MyKeyedService>(new Category()
        {
            Id = 1,
            Name = "test"
        }).Name);
    }
    
    public static async Task WebApiSample()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddKeyedSingleton<IIdGenerator, GuidIdGenerator>("guid");
        var app = builder.Build();
        app.Map("/id0", ([FromKeyedServices("guid")]IIdGenerator idGenerator) 
            => Result.Success<string>(idGenerator.NewId()));
        app.Map("/id", (HttpContext httpContext) =>
        {
            var idGenerator = httpContext.RequestServices.GetRequiredKeyedService<IIdGenerator>("guid");
            return Result.Success<string>(idGenerator.NewId());
        });
        await app.RunAsync();
    }
    
    public static void OptionsSample()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.Configure<TotpOptions>(x =>
        {
            x.Salt = "1234";
        });
        serviceCollection.AddKeyedTransient<ITotpService, TotpService>(KeyedService.AnyKey, 
            (sp, key)=>
            new TotpService(sp.GetRequiredService<IOptionsMonitor<TotpOptions>>()
                .Get(key is string name ? name : Options.DefaultName)));
        
        using var services = serviceCollection.BuildServiceProvider();
        var totpService = services.GetRequiredKeyedService<ITotpService>(string.Empty);
        Console.WriteLine("Totp1: {0}", totpService.GetCode("Test1234"));
        var totpService2 = services.GetRequiredKeyedService<ITotpService>("test");
        Console.WriteLine("Totp2: {0}", totpService2.GetCode("Test1234"));
    }
}

file interface IUserIdProvider
{
    string GetUserId();
}
file class EnvUserIdProvider: IUserIdProvider
{
    public string GetUserId() => Environment.MachineName;
}
file class NullUserIdProvider: IUserIdProvider
{
    public string GetUserId() => "(null)";
}

file sealed class MyNamedService
{
    public MyNamedService([ServiceKey]string name)
    {
        Name = name;
    }

    public string Name { get; }
}

file sealed class MyKeyedService
{
    private readonly Category _category;

    public MyKeyedService([ServiceKey]Category category)
    {
        _category = category;
    }

    public string Name => _category.Name;
    public int Id => _category.Id;
}
