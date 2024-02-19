using System.Text.Json;

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
}
