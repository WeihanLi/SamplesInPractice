namespace BalabalaSample;

public static class ListForEachSample
{
    public static void MainTest()
    {
        var list = Enumerable.Range(1, 5).ToList();
        
        list.ForEach(async i =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"SubTask {i} completed.");
        });

        Console.WriteLine("Task completed");
        Console.ReadLine();
    }

    public static async Task MainTestAsync()
    {
        var list = Enumerable.Range(1, 5).ToList();
        
        foreach (var i in list)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"SubTask {i} completed.");
        }

        Console.WriteLine("Task completed");
        Console.ReadLine();
    }
}
