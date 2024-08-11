using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using WeihanLi.Common.Helpers;

namespace Net9Samples;

public static class JsonSample
{
    public static void MainTest()
    {
        var job = new
        {
            Id = 1,
            Title = "Engineer"
        };

        var indentOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        Console.WriteLine(JsonSerializer.Serialize(job, indentOptions));
        var indentOptions1 = new JsonSerializerOptions
        {
            WriteIndented = true,
            IndentCharacter = '\t',
            IndentSize = 1
        };
        Console.WriteLine(JsonSerializer.Serialize(job, indentOptions1));

        Console.WriteLine();

        Console.WriteLine(JsonSerializer.Serialize(job));
        Console.WriteLine(JsonSerializer.Serialize(job, JsonSerializerOptions.Web));
    }

    public static void JsonSchemaExporterTest()
    {
        var type = typeof(Job);
        var schemaSerializeOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web).WithWriteIntended();

        var schemaNode = JsonSerializerOptions.Web.GetJsonSchemaAsNode(typeof(Job));
        Console.WriteLine(JsonSerializer.Serialize(schemaNode, schemaSerializeOptions));

        var schemaNode2 = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(Job));
        Console.WriteLine(JsonSerializer.Serialize(schemaNode2, schemaSerializeOptions));

        var exporterOptions = new JsonSchemaExporterOptions
        {
            TransformSchemaNode = (context, jsonNode) =>
            {
                var node = jsonNode.DeepClone();
                var idNames = new[] { "id", "Id" };

                if (node["properties"] is not JsonObject propertiesNode)
                    return node;

                foreach (var idName in idNames)
                {
                    if (propertiesNode[idName] is JsonObject)
                    {
                        var requiredNode = node["required"];
                        if (requiredNode is JsonArray jsonArrayNode)
                        {
                            var requiredProperties = JsonSerializer.Serialize(jsonArrayNode.Select(x => x.GetValue<string>()).Append(idName));
                            jsonArrayNode.ReplaceWith(JsonSerializer.Deserialize<JsonArray>(requiredProperties));
                        }
                        else
                        {
                            node["required"] = JsonSerializer.Deserialize<JsonArray>($"""["{idName}"]""");
                        }
                    }

                }
                return node;
            }
        };
        var schemaNode3 = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(Job), exporterOptions);
        Console.WriteLine(JsonSerializer.Serialize(schemaNode3, schemaSerializeOptions));
    }
}


file class Job
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
}
