namespace CSharp10Sample;

public class DeconstructionEnhancementSample
{
    public static void MainTest()
    {
        (var month, var day) = GetDate();
        Console.WriteLine($"Month: {month}, day: {day}");

        (month, var day2) = GetDate();
        Console.WriteLine($"Month: {month}, day: {day2}");

        (month, day) = GetDate();
        Console.WriteLine($"Month: {month}, day: {day}");
    }

    private static (int month, int day) GetDate()
    {
        var today = DateTime.Today;
        return (today.Month, today.Day);
    }
}
