var tasks = new List<Task>();
tasks.AddRange(new[] 
{ 
    Task.Delay(TimeSpan.FromSeconds(5)),
    Task.Delay(TimeSpan.FromSeconds(8)),
    Task.Delay(TimeSpan.FromSeconds(6))
});
Task task = Task.WhenAll(tasks);
try
{
    await task.WaitAsync(TimeSpan.FromSeconds(3));
}
catch (TimeoutException)
{
    Console.WriteLine(nameof(TimeoutException));
}
finally
{
    Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));
    Console.WriteLine(task.Status);
}

await Task.Delay(TimeSpan.FromSeconds(5));
Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));
Console.WriteLine(task.Status);

Console.ReadLine();

var cts = new CancellationTokenSource();
tasks.Clear();
tasks.AddRange(new[]
{
    Task.Delay(TimeSpan.FromSeconds(4)),
    Task.Delay(TimeSpan.FromSeconds(8)),
    Task.Delay(TimeSpan.FromSeconds(6))
});
task = Task.WhenAll(tasks);
try
{
    cts.CancelAfter(TimeSpan.FromSeconds(5));
    await task.WaitAsync(cts.Token);
}
catch (TaskCanceledException)
{
    Console.WriteLine("Task cancelled");
}
finally
{
    Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));
    Console.WriteLine(task.Status);
}

await Task.Delay(TimeSpan.FromSeconds(5));
Console.WriteLine(string.Join(",", tasks.Select(t => t.Status.ToString())));
Console.WriteLine(task.Status);
Console.ReadLine();

try
{
    await Task.Delay(TimeSpan.FromSeconds(5))
      .WaitAsync(TimeSpan.FromSeconds(3), CancellationToken.None);
}
catch(Exception ex)
{
    Console.WriteLine(ex.GetType().Name);
}


try
{
    using var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
    await Task.Delay(TimeSpan.FromSeconds(5))
      .WaitAsync(TimeSpan.FromSeconds(10), cancellationTokenSource.Token);
}
catch(Exception ex)
{
    Console.WriteLine(ex.GetType().Name);
}

Console.ReadLine();
