// See https://aka.ms/new-console-template for more information
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using WeihanLi.Extensions;

Console.WriteLine("Hello, World!");
var text = @".NET 20 周年生日快乐";
const string locale = "zh-CN";

var key = Environment.GetEnvironmentVariable("SpeechSubscriptionKey");
var config = SpeechConfig.FromSubscription(key, "eastasia");
//https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support
config.SpeechSynthesisLanguage = locale;
config.OutputFormat = OutputFormat.Detailed;

// Creates a speech synthesizer
using var synthesizer = new SpeechSynthesizer(config);
using var voicesResult = await synthesizer.GetVoicesAsync();
var voices = voicesResult.Voices
    .Where(x => x.Locale.EqualsIgnoreCase(locale))
    .ToArray();
var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new[] { "en-US", "zh-CN" });
foreach (var voice in voices)
{
    Console.WriteLine($"{voice.Name},{voice.Locale},{voice.LocalName},{voice.VoiceType}");

    config.SpeechSynthesisVoiceName = voice.ShortName;
    var outputFileName = $"output-{voice.ShortName}.wav";
    using (var output = AudioConfig.FromWavFileOutput(outputFileName))
    {
        using var speechSynthesizer = new SpeechSynthesizer(config, output);
        using var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
        Console.WriteLine($"Result: {speechSynthesisResult.Reason}");
    }

    config.SpeechRecognitionLanguage = locale;
    config.RequestWordLevelTimestamps();

    using (var input = AudioConfig.FromWavFileInput(outputFileName))
    {
        using var speechRecognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, input);
        var speechRecognizeResult = await speechRecognizer.RecognizeOnceAsync();
        Console.WriteLine(speechRecognizeResult.Reason);
        Console.WriteLine($"RECOGNIZED: Text = {speechRecognizeResult.Text}");

        Console.WriteLine(" Detailed results:");
        var detailedResult = speechRecognizeResult.Best();
        foreach (var item in detailedResult)
        {
            Console.WriteLine($"\tConfidence: {item.Confidence}\n\tText: {item.Text}\n\tLexicalForm: {item.LexicalForm}\n\tNormalizedForm: {item.NormalizedForm}\n\tMaskedNormalizedForm: {item.MaskedNormalizedForm}");
            Console.WriteLine($"\tWord-level timing:");
            Console.WriteLine($"\t\tWord | Offset | Duration");

            // Word-level timing
            foreach (var word in item.Words)
            {
                Console.WriteLine($"\t\t{word.Word} {word.Offset} {word.Duration}");
            }
        }
    }

    Console.WriteLine("Press 'Enter' to continue");
    Console.ReadLine();
}

Console.WriteLine("Completed");
Console.ReadLine();
