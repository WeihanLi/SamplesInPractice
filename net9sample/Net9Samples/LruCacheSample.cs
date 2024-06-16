using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WeihanLi.Common.Helpers;

namespace Net9Samples;
internal static class LruCacheSample
{
    public static void MainTest()
    {
        var cache = new LruCache(2);
        cache.Set("name", "test");
        cache.Set("age", "10");
        cache.PrintKeys();

        ConsoleHelper.HandleInputLoop(x =>
        {
            if (x.StartsWith("get:"))
            {
                var key = x["get:".Length..];
                cache.TryGetValue(key, out string? value);
                Console.WriteLine($"key: {key}, value: {value}");
                return;
            }

            cache.Set(x, "Hello .NET");

            cache.PrintKeys();
        }, "Input something to test lruCache,  starts with 'get:' to try get cache value, otherwise set cache, input q to exit");
        ConsoleHelper.ReadLineWithPrompt();
    }
}

file interface ICache
{
    void PrintKeys();
    void Set(string key, object? value);
    bool TryGetValue<TValue>(string key, [MaybeNullWhen(false)] out TValue value);
}

file sealed class LastUsedCache(int maxSize) : ICache
{
    private readonly ConcurrentDictionary<string, object?> _store = new();
    private readonly PriorityQueue<string, long> _priorityQueue = new PriorityQueue<string, long>();

    public ICollection<string> Keys => _store.Keys;

    public void PrintKeys()
    {
        Console.WriteLine("PrintKeys:");
        Console.WriteLine(JsonSerializer.Serialize(_store));
    }

    public void Set(string key, object? value)
    {
        if (_store.ContainsKey(key))
        {
            _store[key] = value;
            UpdateKeyAccess(key);
            return;
        }

        while (_store.Count >= maxSize)
        {
            var keyToRemove = _priorityQueue.Dequeue();
            _store.TryRemove(keyToRemove, out _);
        }

        _store[key] = value;
        UpdateKeyAccess(key);
    }

    public bool TryGetValue<TValue>(string key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_store.TryGetValue(key, out var cacheValue))
        {
            value = default;
            return false;
        }

        UpdateKeyAccess(key);

        value = (TValue)cacheValue!;
        return true;
    }

    private void UpdateKeyAccess(string key)
    {
        _priorityQueue.Remove(key, out _, out _);
        _priorityQueue.Enqueue(key, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}

file sealed record CacheAccessEntry
{
    public int AccessCount { get; set; }
    public long AccessTimestamp { get; set; }
}

file sealed class CacheAccessEntryComparer(Func<CacheAccessEntry, long> priorityFactory) : IComparer<CacheAccessEntry>
{
    public CacheAccessEntryComparer() : this(e => e.AccessTimestamp + e.AccessCount)
    {
    }

    public int Compare(CacheAccessEntry? x, CacheAccessEntry? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        return priorityFactory(x).CompareTo(priorityFactory(y));
    }
}

file sealed class LruCache(int maxSize) : ICache
{
    private readonly ConcurrentDictionary<string, object?> _store = new();
    private static readonly CacheAccessEntryComparer CacheEntryComparer = new();
    private readonly PriorityQueue<string, CacheAccessEntry> _priorityQueue =
        new(CacheEntryComparer);

    public ICollection<string> Keys => _store.Keys;

    public void PrintKeys()
    {
        Console.WriteLine("PrintKeys:");
        Console.WriteLine(JsonSerializer.Serialize(_store));
    }

    public void Set(string key, object? value)
    {
        if (_store.ContainsKey(key))
        {
            _store[key] = value;
            UpdateKeyAccess(key);
            return;
        }

        while (_store.Count >= maxSize)
        {
            var keyToRemove = _priorityQueue.Dequeue();
            _store.TryRemove(keyToRemove, out _);
        }

        _store[key] = value;
        UpdateKeyAccess(key);
    }

    public bool TryGetValue<TValue>(string key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_store.TryGetValue(key, out var cacheValue))
        {
            value = default;
            return false;
        }

        UpdateKeyAccess(key);

        value = (TValue)cacheValue!;
        return true;
    }

    private void UpdateKeyAccess(string key)
    {
        _priorityQueue.Remove(key, out _, out var entry);

        entry ??= new();
        entry.AccessTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        entry.AccessCount += 1;
        _priorityQueue.Enqueue(key, entry);
    }
}
