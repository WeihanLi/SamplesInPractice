using System.Diagnostics;

namespace Net8Sample;

public static class TimeProviderSample
{
    public static void MainTest()
    {
        var now = TimeProvider.System.GetUtcNow();
        Console.WriteLine(now.ToString());
        Console.WriteLine(TimeProvider.System.GetLocalNow());
        Console.WriteLine();
        
        var mockTimeProvider = new MockTimeProvider(now.AddYears(100));
        Console.WriteLine(mockTimeProvider.GetUtcNow());
        Console.WriteLine(mockTimeProvider.GetLocalNow());
        Console.WriteLine();
        
        // timestamp
        var startTimestamp = TimeProvider.System.GetTimestamp();
        Thread.Sleep(1000);
        var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);
        Console.WriteLine(elapsedTime);
        Console.WriteLine();
        
        // mocked timestamp
        var startTimestamp1 = mockTimeProvider.GetTimestamp();
        Thread.Sleep(3000);
        var elapsedTime1 = mockTimeProvider.GetElapsedTime(startTimestamp1);
        Console.WriteLine(elapsedTime1);
        Console.WriteLine();

        // mocked timer, Task.Delay
        Console.WriteLine(TimeProvider.System.GetUtcNow());
        Task.Delay(TimeSpan.FromSeconds(1), mockTimeProvider).Wait();
        Console.WriteLine(TimeProvider.System.GetUtcNow());
        
        Console.WriteLine();
    }
}

file sealed class MockTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _dateTimeOffset;

    public MockTimeProvider(DateTimeOffset dateTimeOffset)
    {
        _dateTimeOffset = dateTimeOffset;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _dateTimeOffset;
    }

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override long TimestampFrequency => Stopwatch.Frequency * 3;    

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new MockTimer(callback, state, dueTime, period);
    }
}

file sealed class MockTimer : ITimer
{
    private readonly ITimer _time;

    public MockTimer(
        TimerCallback callback,
        object? state,
        TimeSpan dueTime,
        TimeSpan period)
    {
        _time = TimeProvider.System.CreateTimer(callback, state, dueTime * 2, period);
    }
    
    public void Dispose()
    {
        _time.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _time.DisposeAsync();
    }
    
    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return _time.Change(dueTime, period);
    }
}
