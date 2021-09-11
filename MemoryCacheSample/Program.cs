await using var services = new ServiceCollection()
    .AddMemoryCache()
    .BuildServiceProvider();

Console.WriteLine("----- Bad -----");
GetValidValues_Bad(5).Dump();
GetValidValues_Bad(8).Dump();

Console.WriteLine("----- Good -----");
GetValidValues_Good(5).Dump();
GetValidValues_Good(8).Dump();

List<int> GetValidValues_Bad(int threhold)
{
    var memoryCache = services.GetRequiredService<IMemoryCache>();
    var values = memoryCache.GetOrCreate("test1", entry =>
    {
        return Enumerable.Range(1, 10).ToList();
    });
    values.RemoveAll(x => x > threhold);
    return values;
}

List<int> GetValidValues_Good(int threhold)
{
    var memoryCache = services.GetRequiredService<IMemoryCache>();
    var values = memoryCache.GetOrCreate("test", entry =>
    {
        return Enumerable.Range(1, 10).ToList();
    });
    return values.Where(v => v <= threhold).ToList();
}
