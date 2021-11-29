// See https://aka.ms/new-console-template for more information
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
try
{
    while (await timer.WaitForNextTickAsync(cts.Token))
    {
        Console.WriteLine($"Timed event triggered({DateTime.Now:HH:mm:ss})");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled");
}
Console.WriteLine("Hello world");
