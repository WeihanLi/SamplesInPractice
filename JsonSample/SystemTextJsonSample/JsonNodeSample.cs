using WeihanLi.Common;

namespace SystemTextJsonSample;

public class JsonNodeSample
{
    public static void MainTest()
    {
        var jsonString = JsonSerializer.Serialize(new { Name = "test" });
        WriteLine(jsonString);

        var jsonObject = new JsonObject();
        var jsonNode = JsonNode.Parse(jsonString);
        jsonObject["Test"] = jsonNode;
        Guard.NotNull(jsonNode);
        var jsonNodeJsonString = jsonObject.ToJsonString(new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        WriteLine(jsonNodeJsonString);
    }

    private class TestModelA
    {
        public string? Name { get; set; }

        public object? Child { get; set; }
    }

    private class TestModelB
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
