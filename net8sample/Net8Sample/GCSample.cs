// https://github.com/dotnet/runtime/issues/70601
// https://learn.microsoft.com/en-us/dotnet/api/System.GC.RefreshMemoryLimit?view=net-8.0

using System.Text.Json;

namespace Net8Sample;

public static class GCSample
{
    public static void MainTest()
    {
        PrintMemoryInfo();
        
        ulong memoryInBytes = 50 * 1024 * 1024;
        SetLimitAndRefresh(memoryInBytes);
        
        memoryInBytes = 60 * 1024 * 1024;
        SetLimitAndRefresh(memoryInBytes);

        // System.InvalidOperationException: RefreshMemoryLimit failed with too low hard limit.
        try
        {
            memoryInBytes = 1 * 1024;
            SetLimitAndRefresh(memoryInBytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void SetLimitAndRefresh(ulong limit)
    {
        // - GCHeapHardLimit
        // - GCHeapHardLimitPercent
        // - GCHeapHardLimitSOH
        // - GCHeapHardLimitLOH
        // - GCHeapHardLimitPOH
        // - GCHeapHardLimitSOHPercent
        // - GCHeapHardLimitLOHPercent
        // - GCHeapHardLimitPOHPercent
        AppContext.SetData("GCHeapHardLimit", limit);
        GC.RefreshMemoryLimit();

        PrintMemoryInfo();
    }

    private static void PrintMemoryInfo()
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        Console.WriteLine($"{nameof(memoryInfo.TotalAvailableMemoryBytes)}: {memoryInfo.TotalAvailableMemoryBytes/1024/1024}*1024*1024");
    }
}
