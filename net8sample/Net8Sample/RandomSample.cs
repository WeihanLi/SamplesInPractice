using System.Text.Json;

namespace Net8Sample;

public static class RandomSample
{
    private static readonly char[] ConstantNumbers = new[]
    {
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9'
    };
    
    public static void MainTest()
    {
        GetItems();
        
        Shuffle();
    }

    public static void GetItems()
    {
        // GetItems
        var randomCodeChars = Random.Shared.GetItems(ConstantNumbers, 6);
        var randomCode = new string(randomCodeChars);
        Console.WriteLine(randomCode);
        
        // GetItems 2
        var charArray = new char[6]; 
        Random.Shared.GetItems(ConstantNumbers, charArray.AsSpan());
        Console.WriteLine(new string(charArray));
    }

    public static void Shuffle()
    {
        // Shuffle
        var nums = Enumerable.Range(1, 10).ToArray();
        Random.Shared.Shuffle(nums);
        Console.WriteLine($"Numbers shuffled:\n {JsonSerializer.Serialize(nums)}");
    }
}
