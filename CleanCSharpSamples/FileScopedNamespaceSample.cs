namespace CleanCSharpSamples;

public class FileScopedNamespaceSample
{
    public static void MainTest()
    {
        if (OperatingSystem.IsWindows())
        {
            Console.WriteLine("Running on Windows");
        }
        else
        {
            Console.WriteLine("Running on Non-Windows");
        }
    }
}
