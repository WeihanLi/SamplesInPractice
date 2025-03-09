using System.Text.Json;
using System.Text.Json.Serialization;

namespace Net10Samples;

public static class JsonSamples
{
    public static void JsonIgnoreWhenReadWriteSample()
    {
        var user1 = new User(1, "Mike");
        var user2 = new User2(1, "Mike");
        var user3 = new User3(1, "Mike", "Michael");
        
        {
            Console.WriteLine("JsonIgnoreWhenReadWriteSample");

            Console.WriteLine("User1");
            var json1 = JsonSerializer.Serialize(user1);
            Console.WriteLine(json1);
            var user1Deserialized = JsonSerializer.Deserialize<User>(json1);
            Console.WriteLine(user1Deserialized);
        
            Console.WriteLine("User2");
            var json2 = JsonSerializer.Serialize(user2);
            Console.WriteLine(json2);
            var user2Deserialized = JsonSerializer.Deserialize<User2>(json2);
            Console.WriteLine(user2Deserialized);
            
            Console.WriteLine("User3");
            var json3 = JsonSerializer.Serialize(user3);
            Console.WriteLine(json3);
            var user3Deserialized = JsonSerializer.Deserialize<User3>(json3);
            Console.WriteLine(user3Deserialized);
        }
        Console.WriteLine();
        {
            Console.WriteLine("JsonIgnoreWhenReadWriteSample with generator");
            
            Console.WriteLine("User1");
            var json1 = JsonSerializer.Serialize(user1, UserJsonSerializerContext.Default.User);
            Console.WriteLine(json1);
            var user1Deserialized = JsonSerializer.Deserialize<User>(json1, UserJsonSerializerContext.Default.User);
            Console.WriteLine(user1Deserialized);
        
            Console.WriteLine("User2");
            var json2 = JsonSerializer.Serialize(user2, UserJsonSerializerContext.Default.User2);
            Console.WriteLine(json2);
            var user2Deserialized = JsonSerializer.Deserialize<User2>(json2, UserJsonSerializerContext.Default.User2);
            Console.WriteLine(user2Deserialized);
            
            Console.WriteLine("User3");
            var json3 = JsonSerializer.Serialize(user3, UserJsonSerializerContext.Default.User3);
            Console.WriteLine(json3);
            var user3Deserialized = JsonSerializer.Deserialize<User3>(json3, UserJsonSerializerContext.Default.User3);
            Console.WriteLine(user3Deserialized);
        }
    }
}


sealed record User(int UserId, string UserName);

sealed record User2([property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]int UserId, string UserName);

sealed record User3([property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]int UserId, string UserName, [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]string? Description);


[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(User2))]
[JsonSerializable(typeof(User3))]
internal partial class UserJsonSerializerContext : JsonSerializerContext
{
}
