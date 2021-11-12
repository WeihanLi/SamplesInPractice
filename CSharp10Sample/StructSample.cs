using System.Text.Json;

namespace CSharp10Sample;

public class StructSample
{
    public static void MainTest()
    {
        var p = new Point(1, 2);
        Console.WriteLine(p);
        var p1 = p with { X = 2 };
        Console.WriteLine(p1);

        // Parameterless constructor
        Console.WriteLine(new Point2().ToString());
        Console.WriteLine((new Point2() with { X = 2 }).ToString());
        Console.WriteLine(default(Point2).ToString());

        // More
        var obj = new
        {
            X = 2,
            Y = 2
        };
        Console.WriteLine(JsonSerializer.Serialize(obj));
        Console.WriteLine(JsonSerializer.Serialize(obj with { X = 3, Y = 3 }));

        Console.WriteLine(new RecordClassModel(1, "Test").ToString());
    }

    private record struct Point(int X, int Y);

    private struct Point2
    {
        public int X { get; set; }

        public int Y { get; set; }

        public Point2()
        {
            X = -1;
            Y = -1;
        }

        public override string ToString()
        {
            return $"{X}_{Y}";
        }
    }

    private record class RecordClassModel(int Id, string Name);
}
