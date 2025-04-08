using System.Globalization;
using WeihanLi.Common.Helpers;

namespace Net10Samples;

public class StringComparerSample
{
    public static void MainTest()
    {
        var numericStringComparer = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.NumericOrdering);

        var list = Enumerable.Range(1, 5)
            .Select(n => Random.Shared.Next(3 * n, 100).ToString())
            .ToArray();
        Console.WriteLine(string.Join(", ", list));
        foreach (var item in list.Order(numericStringComparer))
        {
            Console.WriteLine(item);
        }

        Console.WriteLine();
        foreach (var os in new[] { "Windows 8", "Windows 10", "Windows 11" }.Order())
        {
            Console.WriteLine(os);
        }
        Console.WriteLine();
        foreach (var os in new[] { "Windows 8", "Windows 10", "Windows 11" }.Order(numericStringComparer))
        {
            Console.WriteLine(os);
        }

        Console.WriteLine();
        Console.WriteLine(numericStringComparer.Equals("02", "2"));
        
        var ip1 = "127.0.0.1";
        var ip2 = "172.16.0.123";
        var ip3 = "172.16.0.25";

        foreach (var ip in new[]{ ip1, ip2, ip3 }.Order())
        {
            Console.WriteLine(ip);
        }
        Console.WriteLine();
        foreach (var ip in new[]{ ip1, ip2, ip3 }.Order(numericStringComparer))
        {
            Console.WriteLine(ip);
        }
        
        
        ConsoleHelper.ReadLineWithPrompt();
        // Output: True
        
        // Output:
        // Windows 8
        // Windows 10
        // Windows 11

        var set = new HashSet<string>(numericStringComparer) { "007" };
        Console.WriteLine(set.Contains("7"));
        // Output: True
    }
}
