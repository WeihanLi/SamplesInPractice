using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SystemTextJsonSample;

public class CustomConvertSample
{
    public static void MainTest()
    {
        var model = new TestModel
        {
            Id = "123",
            Name = "456"
        };
        var jsonString = JsonSerializer.Serialize(model);
        WriteLine(jsonString);
        var node = JsonNode.Parse(jsonString);
        ArgumentNullException.ThrowIfNull(node, nameof(node));
        node["Id"] = 123;
        var newJsonString = node.ToJsonString();
        WriteLine(newJsonString);
        var newModel = JsonSerializer.Deserialize<TestModel>(newJsonString);
        WriteLine(model == newModel);

        node["Name"] = 345;
        WriteLine(JsonSerializer.Deserialize<TestModel>(node.ToJsonString(), new JsonSerializerOptions
        {
            Converters =
            {
                new StringOrIntConverter()
            }
        })?.Name);
    }
}

public record TestModel
{
    [JsonConverter(typeof(StringOrIntConverter))]
    public string Id { get; init; } = default!;
    public string? Name { get; set; }
}

public class StringOrIntConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32().ToString();
        }
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
