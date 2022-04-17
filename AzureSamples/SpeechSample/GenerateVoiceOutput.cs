using SpeechSample;
using WeihanLi.Extensions;

public class GenerateVoiceOutput
{
    public static async Task MainTest()
    {
        const string txtFilePath = @"C:\Users\Weiha\Desktop\question.txt";
        var index = 0;
        foreach (var line in await File.ReadAllLinesAsync(txtFilePath))
        {
            if (line.IsNullOrWhiteSpace()) continue;
            index++;
            var outputFileName = $"{index}-output.wav";
            await Helper.TextToSpeech(line, outputFileName, "zh-CN-XiaoshuangNeural");
        }
    }
}
