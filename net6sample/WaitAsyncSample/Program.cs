var tasks = new List<Task>();
tasks.AddRange(new[] 
{ 
    Task.Delay(TimeSpan.FromSeconds(5)),
    Task.Delay(TimeSpan.FromSeconds(8)),
    Task.Delay(TimeSpan.FromSeconds(6))
});

try
{
    await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(3));
}
catch (TimeoutException)
{
    Console.WriteLine(nameof(TimeoutException));
}
finally
{
    Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));
}

await Task.Delay(TimeSpan.FromSeconds(5));
Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));

var cts = new CancellationTokenSource();
tasks.Clear();
tasks.AddRange(new[]
{
    Task.Delay(TimeSpan.FromSeconds(5)),
    Task.Delay(TimeSpan.FromSeconds(8)),
    Task.Delay(TimeSpan.FromSeconds(6))
});

try
{
    cts.CancelAfter(TimeSpan.FromSeconds(5));
    await Task.WhenAll(tasks).WaitAsync(cts.Token);
}
catch (TaskCanceledException)
{
    Console.WriteLine("Task cancelled");
}
finally
{
    Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));
}


await Task.Delay(TimeSpan.FromSeconds(5));
Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
Console.ReadLine();
