using System.Globalization;

namespace Net10Samples;

public class StringComparerSample
{
    public static void MainTest()
    {
        var list = Enumerable.Range(1, 5).Select(n => Random.Shared.Next(2 * n, 15).ToString()).ToArray();
        Console.WriteLine(string.Join(", ", list));
        var numbericComparer = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.NumericOrdering);
        foreach (var item in list.Order(numbericComparer))
        {
            Console.WriteLine(item);
        }
        
        //
        StringComparer numericStringComparer = StringComparer.Create(CultureInfo.CurrentCulture, CompareOptions.NumericOrdering);

        Console.WriteLine(numericStringComparer.Equals("02", "2"));
        // Output: True

        foreach (var os in new[] { "Windows 8", "Windows 10", "Windows 11" }.Order(numericStringComparer))
        {
            Console.WriteLine(os);
        }

        // Output:
        // Windows 8
        // Windows 10
        // Windows 11

        HashSet<string> set = new HashSet<string>(numericStringComparer) { "007" };
        Console.WriteLine(set.Contains("7"));
        // Output: True
    }
}
