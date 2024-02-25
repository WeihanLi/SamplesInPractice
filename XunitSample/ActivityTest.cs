using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;

namespace XunitSample;

public class ActivityTest
{
    [Fact]
    public void ActivityListenTest()
    {
        var activities = new List<Activity>();
        
        using var listener = new ActivityListener();
        listener.ShouldListenTo = s => s.Name == nameof(ActivityTest);
        listener.Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData;
        listener.ActivityStopped += a => activities.Add(a);
        ActivitySource.AddActivityListener(listener);

        using var activity = ActivitySource.StartActivity();
        var activityId = activity?.Id ?? string.Empty;
        var traceId = activity?.TraceId.ToHexString();
        
        ActivityDemo();
        
        Assert.NotEmpty(activities);
        var capturedActivity = activities.FirstOrDefault(x => x.TraceId.ToHexString() == traceId);
        Assert.NotNull(capturedActivity);
        Assert.Equal(activityId, capturedActivity.ParentId);
        Assert.Equal(traceId, capturedActivity.TraceId.ToHexString());
        Assert.Equal("1", capturedActivity.GetTagItem("test.id")?.ToString());
    }
    
    private static readonly ActivitySource ActivitySource = new(nameof(ActivityTest));
    private static void ActivityDemo()
    {
        using var activity = ActivitySource.StartActivity();
        Thread.Sleep(100);
        activity?.SetTag("test.id", "1");
    }
}
