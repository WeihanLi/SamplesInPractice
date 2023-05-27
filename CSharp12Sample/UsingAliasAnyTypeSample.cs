using Point = (int X, int Y);

public class UsingAliasAnyTypeSample
{
    public static void MainTest()
    {
        Point point = (3, 4);
        Console.WriteLine($"({point.X}, {point.Y})");
    }
}
