await using var services = new ServiceCollection()
    .AddMemoryCache()
    .BuildServiceProvider();
var memoryCache = services.GetRequiredService<IMemoryCache>();

Task.Run(() =>
{
    memoryCache.GetOrCreate("test", entry =>
    {
        Thread.Sleep(4000);
        Console.WriteLine("completed");
        return "1213";
    });
});
Thread.Sleep(2000);
if (memoryCache.TryGetValue("test", out var value))
{
    Console.WriteLine($"cache get and value is {value}");
}
else
{
    Console.WriteLine("not exists");
}


Console.WriteLine("----- Bad -----");
GetValidValues_Bad(5).Dump();
GetValidValues_Bad(8).Dump();

Console.WriteLine("----- Good -----");
GetValidValues_Good(5).Dump();
GetValidValues_Good(8).Dump();

List<int> GetValidValues_Bad(int threhold)
{
    var values = memoryCache.GetOrCreate("test1", entry =>
    {
        return Enumerable.Range(1, 10).ToList();
    });
    values.RemoveAll(x => x > threhold);
    return values;
}

List<int> GetValidValues_Good(int threhold)
{
    var values = memoryCache.GetOrCreate("test", entry =>
    {
        return Enumerable.Range(1, 10).ToList();
    });
    return values.Where(v => v <= threhold).ToList();
}
