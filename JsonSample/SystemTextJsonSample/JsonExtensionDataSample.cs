namespace SystemTextJsonSample;

public class JsonExtensionDataSample
{
    public static void MainTest()
    {
        TestInternal(JsonSerializer.Serialize(new
        {
            Name = "Ming",
            Age = 10,
        }));

        TestInternal(JsonSerializer.Serialize(new
        {
            Name = "Ming",
            Age = 10,
            Title = "SDE",
            City = "Shanghai"
        }));
    }

    private static void TestInternal(string jsonString)
    {
        WriteLine(jsonString);

        var p = JsonSerializer.Deserialize<Person>(jsonString);
        ArgumentNullException.ThrowIfNull(p, nameof(p));
        WriteLine(JsonSerializer.Serialize(p));

        var p1 = JsonSerializer.Deserialize<Person1>(jsonString);
        ArgumentNullException.ThrowIfNull(p1, nameof(p1));
        WriteLine(JsonSerializer.Serialize(p1));
        WriteLine(JsonSerializer.Serialize(p1.Extensions));

        var p2 = JsonSerializer.Deserialize<Person2>(jsonString);
        ArgumentNullException.ThrowIfNull(p2, nameof(p2));
        WriteLine(JsonSerializer.Serialize(p2));
        WriteLine(JsonSerializer.Serialize(p2.Extensions));

        var p3 = JsonSerializer.Deserialize<Person3>(jsonString);
        ArgumentNullException.ThrowIfNull(p3, nameof(p3));
        WriteLine(JsonSerializer.Serialize(p3));
        WriteLine(JsonSerializer.Serialize(p3.Extensions));

        WriteLine(new string('-', 20));
    }

    public record Person(string Name, int Age);

    public record Person1(string Name, int Age) : Person(Name, Age)
    {
        [JsonExtensionData]
        public Dictionary<string, object?> Extensions { get; set; } = new();
    }

    public record Person2(string Name, int Age) : Person(Name, Age)
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Extensions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public record Person3(string Name, int Age) : Person(Name, Age)
    {
        [JsonExtensionData]
        public JsonObject? Extensions { get; set; }
    }
}
