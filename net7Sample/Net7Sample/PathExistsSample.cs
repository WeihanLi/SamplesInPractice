namespace Net7Sample;

public class PathExistsSample
{
    public static void MainTest()
    {
        var currentDir = Directory.GetCurrentDirectory();
        Console.WriteLine(Path.Exists(currentDir));

        var files = Directory.GetFiles(currentDir);
        if (files.Length > 0)
        {
            var fileName = files[0];
            Console.WriteLine(File.Exists(fileName));
            Console.WriteLine(Path.Exists(fileName));
        }
    }
}
