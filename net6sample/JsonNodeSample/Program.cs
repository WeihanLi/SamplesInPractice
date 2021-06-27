using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using WeihanLi.Extensions;

var testObj = new {Name = "Ming", Age = 10};
var jsonString = JsonSerializer.Serialize(testObj);
var jsonNode = JsonNode.Parse(jsonString);
if (jsonNode is JsonObject jsonObject)
{
    jsonObject["Name"]?.GetValue<string>().Dump();
    jsonObject["Age"]?.GetValue<int>().Dump();
}

//
var testArrayJsonString = JsonSerializer.Serialize(new[]
{
    new {Name = "Ming", Age = 10}, new {Name = "Alice", Age = 6}, new {Name = "Anna", Age = 8}
});
jsonNode = JsonNode.Parse(testArrayJsonString);
if (jsonNode is JsonArray jsonArray)
{
    jsonArray.Select(item => $"{item["Name"]}, {item["Age"]}")
        .StringJoin(Environment.NewLine)
        .Dump();
}

var complexObj = new
{
    Name = "Mike", 
    Users = new[] {new {Name = "Alice", Age = 6}, new {Name = "Anna", Age = 8}}
};
jsonString = JsonSerializer.Serialize(complexObj);
jsonNode = JsonNode.Parse(jsonString);
jsonNode?["Users"]?.AsArray().Select(item => $"{item["Name"]}, {item["Age"]}")
    .StringJoin(Environment.NewLine)
    .Dump();

Console.WriteLine("Hello Amazing .NET");
