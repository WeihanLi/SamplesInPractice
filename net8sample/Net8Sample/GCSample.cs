namespace Net8Sample;
// https://github.com/dotnet/runtime/issues/70601
public static class GCSample
{
    public static void MainTest()
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        Console.WriteLine(memoryInfo.TotalAvailableMemoryBytes);
        
        var memoryInBytes = (ulong)50 * 1024 * 1024;

        // If the user has already configured the hard heap limit to something lower then is available 
        // then make no adjustments to honor the user's setting.
        if ((ulong)memoryInfo.TotalAvailableMemoryBytes < memoryInBytes)
            return;

        AppContext.SetData("GCHeapHardLimit", memoryInBytes);
        
        GC.RefreshMemoryLimit();

        memoryInfo = GC.GetGCMemoryInfo();
        Console.WriteLine(memoryInfo.TotalAvailableMemoryBytes);
    }
}
