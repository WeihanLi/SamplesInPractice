using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WeihanLi.Extensions;


JsonFormatter.JsonNetSample();
JsonFormatter.JsonNetSample2();
JsonFormatter.SystemJsonSample();

// Parse a JSON object
var jNode = JsonNode.Parse(@"{""MyProperty"":42}");
var jValue = jNode["MyProperty"].AsValue();

var value = (int)jValue;
Debug.Assert(value == 42);

// or
value = jValue.GetValue<int>();
Debug.Assert(value == 42);


var testObj = new {Name = "Ming", Age = 10};
var jsonString = JsonSerializer.Serialize(testObj);
var jsonNode = JsonNode.Parse(jsonString);
if (jsonNode is JsonObject jsonObject)
{
    jsonObject["Name"]?.GetValue<string>().Dump();
    jsonObject["Age"]?.GetValue<int>().Dump();

    jsonObject["Name"] = "Michael";
    jsonObject.ToJsonString().Dump();
}

// JSON array
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
    Users = new[]
    {
        new {Name = "Alice", Age = 6}, 
        new {Name = "Anna", Age = 8}
    }
};
jsonString = JsonSerializer.Serialize(complexObj);
jsonString.Dump();

jsonNode = JsonNode.Parse(jsonString);
jsonNode?["Users"]?.AsArray().Select(item => $"--{item["Name"]}, {item["Age"]}")
    .StringJoin(Environment.NewLine)
    .Dump();

jsonNode["Users"][0]["Name"].GetPath().Dump();
// jsonNode[jsonPath].ToString().Dump();

JsonNode.Parse(@"{""Prop1"":1}")["Prop1"].GetPath().Dump();


Console.WriteLine("Hello Amazing .NET");
