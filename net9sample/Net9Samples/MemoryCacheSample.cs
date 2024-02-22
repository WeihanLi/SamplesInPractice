using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Net9Samples;

public static class MemoryCacheSample
{
    public static void MainTest()
    {
        // MemoryCacheKeysSample();
        // MemoryCacheKeysSample2();

        MemoryCacheGetOrCreateExtensionSample();
    }

    // https://github.com/dotnet/runtime/issues/36554
    // https://github.com/dotnet/runtime/pull/94992/files
    public static void MemoryCacheKeysSample()
    {
        Console.WriteLine("MemoryCache keys dependency injection sample");
        using var services = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider();
        var memoryCache = services.GetRequiredService<IMemoryCache>();
        memoryCache.Set("user:1:roles", "user,manager");
        memoryCache.Set("user:1:permission", "read,write");
        memoryCache.Set("user:2:roles", "user");
        memoryCache.Set("user:2:permission", "read");
        foreach (var cacheKey in ((MemoryCache)memoryCache).Keys)
        {
            Console.WriteLine(cacheKey.ToString());
            if (cacheKey is string cacheKeyStr && cacheKeyStr.StartsWith("user:2:"))
            {
                memoryCache.Remove(cacheKey);
            }
        }

        Console.WriteLine("some keys had been removed");
        foreach (var cacheKey in ((MemoryCache)memoryCache).Keys)
        {
            Console.WriteLine(cacheKey.ToString());
        }
    }
    
    public static void MemoryCacheKeysSample2()
    {
        Console.WriteLine("MemoryCache keys non-di sample");
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCache.Set("user:1:roles", "user,manager");
        memoryCache.Set("user:1:permission", "read,write");
        memoryCache.Set("user:2:roles", "user");
        memoryCache.Set("user:2:permission", "read");
        foreach (var cacheKey in memoryCache.Keys)
        {
            Console.WriteLine(cacheKey.ToString());
            if (cacheKey is string cacheKeyStr && cacheKeyStr.StartsWith("user:2:"))
            {
                memoryCache.Remove(cacheKey);
            }
        }

        Console.WriteLine("some keys had been removed");
        foreach (var cacheKey in ((MemoryCache)memoryCache).Keys)
        {
            Console.WriteLine(cacheKey.ToString());
        }
    }

    // https://github.com/dotnet/runtime/issues/92101
    // https://github.com/dotnet/runtime/pull/94335
    public static void MemoryCacheGetOrCreateExtensionSample()
    {
        Console.WriteLine("MemoryCache keys dependency injection sample");
        using var services = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider();
        var memoryCache = services.GetRequiredService<IMemoryCache>();
        
        var key = "user:2:roles";
        FetchValue();
        Thread.Sleep(1000);
        FetchValue();
        Thread.Sleep(3000);
        FetchValue();

        void FetchValue()
        {    
            memoryCache.GetOrCreate("no-expiration", _ =>
                {
                    Console.WriteLine("fetch value from source for no-expiration key");
                    return "user";
                });
            
            memoryCache.GetOrCreate(key, _ =>
                {
                    Console.WriteLine("fetch value from source");
                    return "user";
                },
                new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3) });
        }
    }
}
