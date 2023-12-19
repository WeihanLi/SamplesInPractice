using System.Diagnostics;

namespace TestLibrary;

public sealed class ScopeActivityCreator : IDisposable, IAsyncDisposable
{
    private static readonly ActivitySource ActivitySource = new(nameof(ScopeActivityCreator));

    private readonly Activity? _activity = ActivitySource.StartActivity(nameof(ScopeActivityCreator));

    public Activity? Activity => _activity;
    
    public void Dispose()
    {
        Console.WriteLine("Dispose ing...");
        _activity?.Dispose();
        Console.WriteLine("Dispose done");
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("DisposeAsync ing...");
        _activity?.Dispose();
        Console.WriteLine("DisposeAsync done");
        return ValueTask.CompletedTask;
    }
}

