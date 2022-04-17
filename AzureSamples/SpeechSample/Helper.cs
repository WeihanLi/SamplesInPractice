using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace SpeechSample;

public static class Helper
{
    public static SpeechConfig SpeechConfig { get; private set; } = default!;

    public static void RegisterSpeechConfig(string key, string region = "eastasia")
    {
        SpeechConfig = SpeechConfig.FromSubscription(key, region);
        SpeechConfig.OutputFormat = OutputFormat.Detailed;
    }

    public static async Task TextToSpeech(string text, string outputPath, string voiceName)
    {
        SpeechConfig.SpeechSynthesisVoiceName = voiceName;
        using var synthesizer = new SpeechSynthesizer(SpeechConfig, AudioConfig.FromWavFileOutput(outputPath));
        using var result = await synthesizer.SpeakTextAsync(text);
        Console.WriteLine($"{outputPath} {result.Reason}");
    }
}
