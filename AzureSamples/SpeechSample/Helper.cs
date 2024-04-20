using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace SpeechSample;

public static class Helper
{
    public static SpeechConfig SpeechConfig { get; private set; } = default!;

    public static void RegisterSpeechConfig(string? key = null, string region = "eastasia")
    {
        key ??= Environment.GetEnvironmentVariable("SpeechSubscriptionKey");
        SpeechConfig = SpeechConfig.FromSubscription(key, region);
        SpeechConfig.OutputFormat = OutputFormat.Detailed;
        SpeechConfig.SpeechRecognitionLanguage = "en-US";
    }

    public static async Task TextToSpeech(string text, string outputPath, string voiceName)
    {
        SpeechConfig.SpeechSynthesisVoiceName = voiceName;
        using var synthesizer = new SpeechSynthesizer(SpeechConfig, AudioConfig.FromWavFileOutput(outputPath));
        using var result = await synthesizer.SpeakTextAsync(text);
        Console.WriteLine($"{outputPath} {result.Reason}");
    }

    public static Stream VideoToAudio(string videoPath)
    {
        var tempPath = Path.GetTempFileName();
        FFMpeg.ExtractAudio(videoPath, tempPath);
        return File.OpenRead(tempPath);
    }
}
