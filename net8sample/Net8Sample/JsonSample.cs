using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WeihanLi.Common.Helpers;

namespace Net8Sample;

public static class JsonSample
{
    public static void MainTest()
    {
        MissingMemberHandlingTest();

        InterfaceHierarchyTest();
        
        SnakeCaseNamingTest();
        KebabCaseNamingTest();

        JsonSerializerOptionsReadOnlyTest();

        EnumerableTest();
        DeepCloneEqualsTest();

        JsonNodeEnrichment();
    }

    private static void MissingMemberHandlingTest()
    {
        var personJsonWithoutId = JsonSerializer.Serialize(new { Id = 1, Name = "1234", Age = 10 });
        
        try
        {
            var p = JsonSerializer.Deserialize<Person>(personJsonWithoutId);
            Console.WriteLine(p?.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        try
        {
            var p = JsonSerializer.Deserialize<Person>(personJsonWithoutId,
                new JsonSerializerOptions() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow });
            Console.WriteLine(p?.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        try
        {
            var p = JsonSerializer.Deserialize<Person2>(personJsonWithoutId);
            Console.WriteLine(p?.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        try
        {
            var p = JsonSerializer.Deserialize<PersonWithExtensionData>(personJsonWithoutId,
                new JsonSerializerOptions() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow });
            Console.WriteLine(JsonSerializer.Serialize(p));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void InterfaceHierarchyTest()
    {
        IDerived value = new DerivedImplement() { Base = 0, Derived =1 };
        var serializedValue = JsonSerializer.Serialize(value);
        Console.WriteLine(serializedValue);
    }
    
    private static void SnakeCaseNamingTest()
    {
        var p = new Person2() 
        { 
            Id = 1, 
            Name = "Alice",
            JobTitle = "Engineer" 
        };
        var snakeCaseLowerJson = JsonSerializer.Serialize(p, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        Console.WriteLine(snakeCaseLowerJson);
        
        var snakeCaseUpperJson = JsonSerializer.Serialize(p, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper
        });
        Console.WriteLine(snakeCaseUpperJson);
    }
    private static void KebabCaseNamingTest()
    {
        var p = new Person2() 
        { 
            Id = 1, 
            Name = "Alice",
            JobTitle = "Engineer" 
        };
        var kebabCaseLowerJson = JsonSerializer.Serialize(p, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
        });
        Console.WriteLine(kebabCaseLowerJson);
        
        var kebabCaseUpperJson = JsonSerializer.Serialize(p, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper
        });
        Console.WriteLine(kebabCaseUpperJson);
    }

    private static void JsonSerializerOptionsReadOnlyTest()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        Console.WriteLine($"IsReadOnly: {options.IsReadOnly}");
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        Console.WriteLine("PropertyNamingPolicy updated");
        
        options.MakeReadOnly();
        Console.WriteLine($"IsReadOnly: {options.IsReadOnly}");

        try
        {
            options.PropertyNamingPolicy = null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void EnumerableTest()
    {
        var jsonArray = new JsonArray(1, 2, 3, 2);
        // With the proposed GetValues this is simpler:
        var values = jsonArray.GetValues<int>()
            .Where(i => i != 2);
        foreach (var value in values)
        {
            Console.WriteLine(value);
        }
    }
    
    private static void DeepCloneEqualsTest()
    {
        var node = JsonNode.Parse("""{"id": 1, "name": "test", "jobs": ["abc", "def"]}""");
        ArgumentNullException.ThrowIfNull(node);
        var node2 = node.DeepClone();
        Console.WriteLine(node2.ToJsonString());
        Console.WriteLine("JsonNode.DeepEquals(node, node2) ?? {0}", JsonNode.DeepEquals(node, node2));
    }

    private static void JsonNodeEnrichment()
    {
        var node = JsonNode.Parse("""{"id": 1, "name": "test", "jobs": ["abc", "def"]}""");
        ArgumentNullException.ThrowIfNull(node);
        Console.WriteLine(node.GetValueKind());
        InvokeHelper.TryInvoke(() => Console.WriteLine(node.GetElementIndex()));
        InvokeHelper.TryInvoke(() => Console.WriteLine(node.GetPropertyName()));
        Console.WriteLine(node["id"]?.GetPropertyName());
        Console.WriteLine(node["jobs"]?.AsArray()[1]?.GetElementIndex());
    }
}

file record Person(int Id, string Name);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
file record Person2
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? JobTitle { get; set; }
}

file record PersonWithExtensionData
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    [JsonExtensionData]
    public Dictionary<string,object>? Extensions { get; set; }
}

file interface IBase
{
    int Base { get; set; }
}
file interface IDerived : IBase
{
    int Derived { get; set; }
}
file class DerivedImplement : IDerived
{
    public int Base { get; set; }
    public int Derived { get; set; }
}

