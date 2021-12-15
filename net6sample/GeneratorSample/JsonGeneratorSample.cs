using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeneratorSample;

public partial class JsonGeneratorSample
{
    public static void MainTest()
    {
        var person = new Person
        {
            FirstName = "Alice",
            LastName = "Blue"
        };
        Console.WriteLine(person.ToString());
        var json = JsonSerializer.Serialize(person, PersonJsonContext.Default.Options);
        Console.WriteLine(json);
        var person1 = JsonSerializer.Deserialize<Person>(json, PersonJsonContext.Default.Options);
        ArgumentNullException.ThrowIfNull(person1);
        Console.WriteLine(person1.ToString());
        OutputCompareResult(person == person1);
    }

    private static void OutputCompareResult(bool result, [CallerArgumentExpression("result")] string? expression = null)
    {
        Console.WriteLine($"{expression}: {result}");
    }

    private record Person
    {
        //init-only properties, deserialization of which is currently not supported in source generation mode

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(Person))]
    private partial class PersonJsonContext : JsonSerializerContext
    {
    }
}
