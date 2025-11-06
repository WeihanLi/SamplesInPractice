using Sdcb.WordClouds;
using SkiaSharp;
using System.Text.Json;

namespace BalabalaSample;

public class DotnetConfAgendaAnalyzer
{
    public static async Task RunAsync()
    {
        await using var fs = File.OpenRead("dotnetconf2025-agenda.json");
        var sessions = JsonSerializer.Deserialize<SessionModel[]>(fs);
        ArgumentNullException.ThrowIfNull(sessions);
        var personNames = sessions.SelectMany(s=> s.Speaker.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)).ToHashSet();
        var excludeWords = new HashSet<string>([
            "a", "an", "the", "and", "but", "which", "is", "session", "to", "of", "for", "in", "or", "on", "for", "you", "your", "we", "our", "with",
            "this", "that", "can", "could", "will", "would", "as", "into", "it", "be", "are", "us", "use", "talk", "we'll", "such", "about",
            "some", "also", "show", "visual", "studio", "I", "time", "has", "have", "had", "even", "get", "its", "start", "dive", "do", "up", "out", "been", "be",
            "end", "most", "they", "it's", "whether", "of", "instead", "so", "&", "we'll", "at", "by", "take", "now", "next", "other", "others", "if", "demo",
            "here", "there", "demos", "from", "to", "join", "welcome"
        ], StringComparer.OrdinalIgnoreCase);
        var scores = sessions.SelectMany(s => s.Description.Split([',', ' ', '.', ';'], 
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries ))
            .Where(x=> !personNames.Contains(x) && !excludeWords.Contains(x))
            .GroupBy(x => x.Trim().ToLowerInvariant())
            .Select(x => new WordScore(
                x.Key.Length <=3 ? x.Key.ToUpper() : x.Key, 
                x.Count()))
            .OrderByDescending(x=> x.Score)
            .Where(x => x.Score > 4)
            .ToArray();
        
        var wordCloudOptions = new WordCloudOptions(1024, 1024, scores)
        {
            FontManager = new FontManager([SKTypeface.FromFamilyName("JetBrains Mono")])
        };
        var wordCloud = WordCloud.Create(wordCloudOptions);
        var pngBytes = wordCloud.ToSKBitmap()
            .Encode(SKEncodedImageFormat.Jpeg, 100)
            .AsSpan().ToArray();
        await File.WriteAllBytesAsync("dotnetconf-wordcloud2.png", pngBytes);
    }
}
