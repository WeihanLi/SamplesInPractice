using System.Text.Json;
using System.Text.Json.Serialization;
using WeihanLi.Extensions;

namespace Net10Samples;

public static class JsonSamples
{
    public static void JsonIgnoreWhenReadWriteSample()
    {
        var user1 = new User(1, "Mike");
        var user2 = new User2(1, "Mike");
        var user3 = new User3(1, "Mike", "Michael");
        var user4 = new User4 { UserId = 1, UserName = "Alice", Description = "Alice " };
        var p1 = new Person
        {
            Id = 1,
            Name = "Jane",
            Description = "Jane J"
        };
        
        Console.WriteLine("JsonIgnoreWhenReadWriteSample");
        
        Console.WriteLine("Person1");
        Console.WriteLine(p1.ToJson());
        var jsonP1 = JsonSerializer.Serialize(p1);
        Console.WriteLine(jsonP1);
        var p1Deserialized = JsonSerializer.Deserialize<Person>(jsonP1);
        Console.WriteLine(p1Deserialized.ToJson());

        Console.WriteLine("User1");
        Console.WriteLine(user1.ToJson());
        var json1 = JsonSerializer.Serialize(user1);
        Console.WriteLine(json1);
        var user1Deserialized = JsonSerializer.Deserialize<User>(json1);
        Console.WriteLine(user1Deserialized);
        
        Console.WriteLine("User2");
        Console.WriteLine(user2.ToJson());
        var json2 = JsonSerializer.Serialize(user2);
        Console.WriteLine(json2);
        var user2Deserialized = JsonSerializer.Deserialize<User2>(json2);
        Console.WriteLine(user2Deserialized);
            
        Console.WriteLine("User3");
        Console.WriteLine(user3.ToJson());
        var json3 = JsonSerializer.Serialize(user3);
        Console.WriteLine(json3);
        var user3Deserialized = JsonSerializer.Deserialize<User3>(json3);
        Console.WriteLine(user3Deserialized);
        
        Console.WriteLine("User4");
        Console.WriteLine(user4.ToJson());
        var json4 = JsonSerializer.Serialize(user4);
        Console.WriteLine(json4);
        var user4Deserialized = JsonSerializer.Deserialize<User4>(json4);
        Console.WriteLine(user4Deserialized);
    }

    public static void JsonIgnoreWithGeneratorTest()
    {
        var user1 = new User(1, "Mike");
        var user2 = new User2(1, "Mike");
        var user3 = new User3(1, "Mike", "Michael");
        var user4 = new User4 { UserId = 1, UserName = "Alice", Description = "Alice " };
        var p1 = new Person
        {
            Id = 1,
            Name = "Jane",
            Description = "Jane J"
        };
        
        Console.WriteLine("JsonIgnoreWhenReadWriteSample with generator");
        
        Console.WriteLine("Person1");
        Console.WriteLine(p1.ToJson());
        var jsonP1 = JsonSerializer.Serialize(p1, CustomJsonSerializerContext.Default.Person);
        Console.WriteLine(jsonP1);
        var p1Deserialized = JsonSerializer.Deserialize<Person>(jsonP1, CustomJsonSerializerContext.Default.Person);
        Console.WriteLine(p1Deserialized.ToJson());

        Console.WriteLine("User1");
        Console.WriteLine(user1.ToJson());
        var json1 = JsonSerializer.Serialize(user1, CustomJsonSerializerContext.Default.User);
        Console.WriteLine(json1);
        var user1Deserialized = JsonSerializer.Deserialize<User>(json1, CustomJsonSerializerContext.Default.User);
        Console.WriteLine(user1Deserialized);
        
        Console.WriteLine("User2");
        Console.WriteLine(user2.ToJson());
        var json2 = JsonSerializer.Serialize(user2, CustomJsonSerializerContext.Default.User2);
        Console.WriteLine(json2);
        var user2Deserialized = JsonSerializer.Deserialize<User2>(json2, CustomJsonSerializerContext.Default.User2);
        Console.WriteLine(user2Deserialized);
            
        Console.WriteLine("User3");
        Console.WriteLine(user3.ToJson());
        var json3 = JsonSerializer.Serialize(user3, CustomJsonSerializerContext.Default.User3);
        Console.WriteLine(json3);
        var user3Deserialized = JsonSerializer.Deserialize<User3>(json3, CustomJsonSerializerContext.Default.User3);
        Console.WriteLine(user3Deserialized);
        
        Console.WriteLine("User4");
        Console.WriteLine(user4.ToJson());
        var json4 = JsonSerializer.Serialize(user4, CustomJsonSerializerContext.Default.User4);
        Console.WriteLine(json4);
        var user4Deserialized = JsonSerializer.Deserialize<User4>(json4, CustomJsonSerializerContext.Default.User4);
        Console.WriteLine(user4Deserialized);
    }
}

sealed class Person
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public int Id { get; set; }
    
    public required string Name { get; set; }

    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public string? Description { get; set; }

    public override string ToString()
    {
        return $"{Id}-{Name}";
    }
}

sealed record User(int UserId, string UserName);

sealed record User2([property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]int UserId, string UserName);

sealed record User3(
    [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]int UserId, 
    string UserName, 
    [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]string? Description
    );

sealed record User4
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public int UserId { get; init; }

    public required string UserName { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public string? Description { get; set; }
}


[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(User2))]
[JsonSerializable(typeof(User3))]
[JsonSerializable(typeof(User4))]
[JsonSerializable(typeof(Person))]
internal partial class CustomJsonSerializerContext : JsonSerializerContext
{
}
