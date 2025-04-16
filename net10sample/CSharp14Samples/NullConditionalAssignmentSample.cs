using System.Diagnostics;

namespace CSharp14Samples;

// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/null-conditional-assignment
// https://github.com/dotnet/csharplang/issues/8677
// https://github.com/dotnet/csharplang/discussions/8676
// https://github.com/dotnet/aspnetcore/pull/61244/files
public class NullConditionalAssignmentSample
{
    public static void Run()
    {
        var p1 = new Person("Alice", 3);
        if (p1 is not null)
        {
            p1.Age = 10;
        }
        Console.WriteLine(p1);
        
        var p2 = new Person("Bob", 2);
        p2?.Age = 20;
        Console.WriteLine(p2);

        p2?.OnAgeChanged += p => Console.WriteLine(p.ToString());

        p2?.Tags?[1] = "test";
    }
}

[DebuggerDisplay("Age = {Age}, Name = {Name,nq}")]
file class Person(string name, int age)
{
    public string Name { get; set; } = name;
    public int Age { get; set; } = age;

    public event Action<Person> OnAgeChanged;

    public string[]? Tags { get; set; }

    public override string ToString() => $"{Name} is {Age} years old.";
}
