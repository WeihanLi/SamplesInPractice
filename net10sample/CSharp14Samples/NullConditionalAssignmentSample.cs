using System.Diagnostics;

namespace CSharp14Samples;

public class NullConditionalAssignmentSample
{
    public static void Run()
    {
        var p1 = new Person("Alice", 3);
        if (p1 is not null)
        {
            p1.Age = 10;
        }
        
        var p2 = new Person("Bob", 2);
        p2?.Age = 20;
        Console.WriteLine(p2);
    }
}

[DebuggerDisplay("Age = {Age}, Name = {Name,nq}")]
file class Person(string name, int age)
{
    public string Name { get; set; } = name;
    public int Age { get; set; } = age;
}
