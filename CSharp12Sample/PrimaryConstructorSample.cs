namespace CSharp12Sample;

public class PrimaryConstructorSample
{
    public static void MainTest()
    {
        var mouse = new Animal("Jerry");
        Console.WriteLine(mouse.Name);
        var cat = new Cat("Tom");
        Console.WriteLine(cat.Name);

        var point = new Point(2, 2);
        Console.WriteLine(point);
    }
}

file class Animal(string name) 
{
    public string Name => name;
}

file sealed class Cat(string Name) : Animal(Name){}

file struct Point(int x, int y)
{
    public int X => x;
    public int Y => y;

    public override string ToString() => $"({X}, {Y})";
}
