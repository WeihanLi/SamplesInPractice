using dotenv.net;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using SpeechSample;
using WeihanLi.Common.Helpers;

DotEnv.Load();
Helper.RegisterSpeechConfig();
var config = Helper.SpeechConfig;

{
    var audioFilePath = @"C:\Users\weiha\Videos\Writing-async_await-from-scratch-0.wav";
    using var auditConfig = AudioConfig.FromWavFileInput(audioFilePath);
    using var speechRecognizer = new SpeechRecognizer(config, auditConfig);

    speechRecognizer.Recognized += (sender, eventArgs) =>
    {
        Console.WriteLine(eventArgs.Result.Reason);
        Console.WriteLine($"RECOGNIZED: Text = {eventArgs.Result.Text}");
        
        Console.WriteLine(" Detailed results:");
        var detailedResult = eventArgs.Result.Best();
        foreach (var item in detailedResult)
        {
            Console.WriteLine(
                $"\tConfidence: {item.Confidence}\n\tText: {item.Text}\n\tLexicalForm: {item.LexicalForm}\n\tNormalizedForm: {item.NormalizedForm}\n\tMaskedNormalizedForm: {item.MaskedNormalizedForm}");
            Console.WriteLine($"\tWord-level timing:");
            Console.WriteLine($"\t\tWord | Offset | Duration");

            // Word-level timing
            foreach (var word in item.Words)
            {
                Console.WriteLine($"\t\t{word.Word} {word.Offset} {word.Duration}");
            }
        }
    };
    await speechRecognizer.StartContinuousRecognitionAsync();
}

ConsoleHelper.ReadLineWithPrompt();

{
    var text = @".NET 20 周年生日快乐";
    const string locale = "zh-CN";
    //https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support
    config.SpeechSynthesisLanguage = locale;
    await GenerateVoiceOutput.MainTest();
    Console.ReadLine();
    
    // Creates a speech synthesizer
    using var synthesizer = new SpeechSynthesizer(config);
    // await synthesizer.SpeakTextAsync(text);
    using var voicesResult = await synthesizer.GetVoicesAsync(locale);
    var voices = voicesResult.Voices;
    var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new[] { "en-US", "zh-CN" });
    foreach (var voice in voices)
    {
        Console.WriteLine($"{voice.Name},{voice.Locale},{voice.LocalName},{voice.VoiceType}");

        config.SpeechSynthesisVoiceName = voice.ShortName;
        var outputFileName = $"output-{voice.ShortName}.wav";
        {
            //using var speechSynthesizer = new SpeechSynthesizer(config);
            //using var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
        }
        {
            using var output = AudioConfig.FromWavFileOutput(outputFileName);
            using var speechSynthesizer = new SpeechSynthesizer(config, output);
            using var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
            Console.WriteLine($"Result: {speechSynthesisResult.Reason}");
        }
        {
            using var streamSynthesizer = new SpeechSynthesizer(config, null);
            var streamResult = await streamSynthesizer.SpeakTextAsync(text);
            using var audioDataStream = AudioDataStream.FromResult(streamResult);
            // SaveToFile
            //await audioDataStream.SaveToWaveFileAsync(outputFileName);

            // Reads data from the stream
            using var ms = new MemoryStream();
            var buffer = new byte[32000];
            uint filledSize;
            while ((filledSize = audioDataStream.ReadData(buffer)) > 0)
            {
                ms.Write(buffer, 0, (int)filledSize);
            }

            Console.WriteLine($"Totally {ms.Length} bytes received.");
        }
        {
            var ssml =
                $@"<speak xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:mstts=""http://www.w3.org/2001/mstts"" xmlns:emo=""http://www.w3.org/2009/10/emotionml"" version=""1.0"" xml:lang=""en-US""><voice name=""zh-CN-XiaoxiaoNeural""><prosody rate=""0%"" pitch=""50%"">
        {text}
        </prosody></voice></speak>";
            using var ssmlSynthesisResult = await synthesizer.SpeakSsmlAsync(ssml);
            Console.WriteLine($"Result: {ssmlSynthesisResult.Reason}");
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
                Console.WriteLine(
                    $"\tConfidence: {item.Confidence}\n\tText: {item.Text}\n\tLexicalForm: {item.LexicalForm}\n\tNormalizedForm: {item.NormalizedForm}\n\tMaskedNormalizedForm: {item.MaskedNormalizedForm}");
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
}
Console.WriteLine("Completed");
Console.ReadLine();
