using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace SystemTextJsonSample;

public class EncoderSample
{
    public static void MainTest()
    {
        var testObj = new
        {
            Name = "小明",
            Age = 10,
            Description = "<h1>这是标题</h1>"
        };
        WriteLine(JsonSerializer.Serialize(testObj));

        WriteLine(JsonSerializer.Serialize(testObj, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
        }));

        WriteLine(JsonSerializer.Serialize(testObj, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, new UnicodeRange(0x4E00, 8000))
        }));

        WriteLine(JsonSerializer.Serialize(testObj, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        }));

        WriteLine(JsonSerializer.Serialize(testObj, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }));
    }
}
