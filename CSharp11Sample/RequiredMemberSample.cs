using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CSharp11Sample;
internal static class RequiredMemberSample
{
    public static void MainTest()
    {
        var p = new Person()
        {
            FirstName = "123",
            LastName = "456",
            Age = 10,
        };
        Console.WriteLine(p);

        var pJsonString = JsonSerializer.Serialize(p);
        Console.WriteLine(pJsonString);
        var pDeserialized = JsonSerializer.Deserialize<Person>(pJsonString);
        Console.WriteLine(p == pDeserialized);

        var s = new Student()
        {
            FirstName = "Ming",
            LastName = "Green",
            Id = 1,
            Description = null
        };
        Console.WriteLine(s);
        var sDeserialized = JsonSerializer.Deserialize<Student>(JsonSerializer.Serialize(s));
        Console.WriteLine(s == sDeserialized);

        var pet = new Pet { Name = "0.0", Age = 1 };
        Console.WriteLine($"{pet.Name} -- {pet.Age}");
        var pet2 = new Pet("test");
        Console.WriteLine($"{pet2.Name} -- {pet2.Age}");
    }

    private sealed class Pet
    {
        public required string Name { get; init; }
        public required int Age { get; init; }
        public Pet()
        {
        }

        [SetsRequiredMembers]
        public Pet(string name) => Name = name;
    }

    private record Person
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public int Age { get; set; }
        public string? Description { get; set; }
    }

    private sealed record Student : Person
    {
        public required int Id { get; set; }
    }
}
