using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeihanLi.Extensions;

internal class JsonFormatter
{
    const string TestJsonString = @"[{""Name"":""Ming"",""Age"":10}]";
    public static void JsonNetSample()
    {
        using var stringReader = new StringReader(TestJsonString);
        using var stringWriter = new StringWriter();
        using var jsonReader = new JsonTextReader(stringReader);
        using var jsonWriter = new JsonTextWriter(stringWriter)
        {
            Formatting = Formatting.Indented
        };
        jsonWriter.WriteToken(jsonReader);
        jsonWriter.Flush();

        var formattedJson = stringWriter.ToString();
        Console.WriteLine(formattedJson);
    }

    public static void JsonNetSample2()
    {
        var formattedJson = JToken.Parse(TestJsonString).ToString(Formatting.Indented);
        Console.WriteLine(formattedJson);
    }

    public static void SystemJsonSample()
    {
        using var doc = JsonDocument.Parse(TestJsonString);
        
        using var ms = new MemoryStream();
        using var jsonWriter = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true });
        doc.WriteTo(jsonWriter);
        jsonWriter.Flush();

        var formattedJson = ms.ToArray().GetString();
        Console.WriteLine(formattedJson);
    }
}

