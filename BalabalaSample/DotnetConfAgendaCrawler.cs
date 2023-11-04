using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace BalabalaSample;

public static class DotnetConfAgendaCrawler
{
    public static async Task MainTest()
    {
        var agendaHtml = await HttpHelper.HttpClient.GetStringAsync("https://dotnetconf.com/agenda");
        var htmlParser = new HtmlParser();
        var doc = await htmlParser.ParseDocumentAsync(agendaHtml);

        var agendaContainerElement = doc.QuerySelector(".agenda-container");
        ArgumentNullException.ThrowIfNull(agendaContainerElement);

        var dayGroupElements = agendaContainerElement.QuerySelectorAll("div[data-day]");
        var groups = new List<GroupModel>();
        foreach (var groupElement in agendaContainerElement.QuerySelectorAll(".agenda-group"))
        {
            var group = new GroupModel()
            {
                Day = Convert.ToInt32(dayGroupElements[groups.Count].GetAttribute("data-day")),
                GroupTitle = dayGroupElements[groups.Count].TextContent.Trim()
                    .Replace("\r\n", "\n")
                    .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .StringJoin(" - ")
            };
            var dateTitleElements = groupElement.QuerySelectorAll("p.agenda-group-title");
            foreach (var sessionElement in groupElement.QuerySelectorAll(".agenda-group-sessions-container"))
            {
                var dateElement = dateTitleElements[group.Sessions.Count].QuerySelector("span[data-start]");
                var startDateString = dateElement?.GetAttribute("data-start");
                ArgumentNullException.ThrowIfNull(startDateString);
                var endDateString = dateElement?.GetAttribute("data-end");
                ArgumentNullException.ThrowIfNull(endDateString);
                var pstOffset = TimeSpan.FromHours(-7);
                var startDate = new DateTimeOffset(DateTime.Parse(startDateString.Replace("PST", "").Trim()), pstOffset);
                var endDate = new DateTimeOffset(DateTime.Parse(endDateString.Replace("PST", "").Trim()), pstOffset);
                var session = new SessionModel()
                {
                    SessionId = Convert.ToInt32(sessionElement.GetAttribute("data-sessionid")),
                    BeginDateTime = startDate,
                    EndDateTime = endDate,
                    Title = sessionElement.QuerySelector(".agenda-title")?.TextContent.Trim() ?? string.Empty,
                    Speaker = sessionElement.QuerySelector(".agenda-speaker-name")?.TextContent.Trim() ?? string.Empty,
                    Description = sessionElement.QuerySelector(".agenda-description")?.TextContent.Trim() ?? string.Empty,
                };
                group.Sessions.Add(session);
            }
            group.Sessions.Sort();
            groups.Add(group);
        }

        if (groups.Count == 0)
        {
            Console.WriteLine("Nothing found");
            return;
        }

        var sessions = groups.SelectMany(s => s.Sessions)
            .OrderBy(x=> x.BeginDateTimeInCST).ThenBy(x=> x.SessionId)
            .ToList();

        var translationHelper = new TranslationHelper();
        foreach (var session in sessions)
        {
            try
            {
                await RetryHelper.TryInvokeAsync(async () =>
                {
                    session.DescriptionInZh = await translationHelper.GetTranslation(session.Description);
                }, 5, (i, _, ex)=>ConsoleHelper.WriteLineWithColor($"Exception, retry count: {i}, {ex}", ConsoleColor.Red), delayFunc: c => TimeSpan.FromSeconds(c * 5));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);   
            }
        }
        
        var sessionsJson = JsonConvert.SerializeObject(sessions, Formatting.Indented);
        Console.WriteLine(sessionsJson);
        await File.WriteAllTextAsync("dotnetconf2023-agenda.json", sessionsJson);
        //
        var markdownSnippets = string.Join(Environment.NewLine, sessions.Select(x => x.GetMarkdownSnippet()));
        Console.WriteLine(markdownSnippets);
        await File.WriteAllTextAsync("dotnetconf2023-agenda.md", markdownSnippets);
    }
}

file sealed class GroupModel
{
    public required int Day { get; set; }
    public required string GroupTitle { get; set; }
    public List<SessionModel> Sessions { get; } = new();
    
    public string GetMarkdownSnippet()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"## {GroupTitle}");
        foreach (var session in Sessions.OrderBy(x=> x.BeginDateTime))
        {
            stringBuilder.AppendLine(session.GetMarkdownSnippet());
            stringBuilder.AppendLine();
        }
        return stringBuilder.ToString();
    }
}

file sealed class SessionModel: IComparable, IComparable<SessionModel>
{
    public required int SessionId { get; init; }
    public required DateTimeOffset BeginDateTime { get; init; }
    public required DateTimeOffset EndDateTime { get; init; }

    /// <summary>
    /// BeginDateTime in ChinaStandardTime
    /// </summary>
    public DateTime BeginDateTimeInCST => BeginDateTime.UtcDateTime.AddHours(8);
    
    /// <summary>
    /// EndDateTime in ChinaStandardTime
    /// </summary>
    public DateTime EndDateTimeInCST => EndDateTime.UtcDateTime.AddHours(8);
    public required string Title { get; set; }
    public required string Speaker { get; set; }
    public required string Description { get; set; }
    
    public string DescriptionInZh { get; set; }

    public int CompareTo(object? obj)
    {
        return ((IComparable)BeginDateTime).CompareTo(obj);
    }

    public int CompareTo(SessionModel? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var result = BeginDateTime.CompareTo(other.BeginDateTime);
        return result is not 0 ? result : SessionId.CompareTo(other.SessionId);
    }
    
    public string GetMarkdownSnippet()
    {
        var session = this;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"### {session.Title}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"> {session.BeginDateTimeInCST.Date:yyyy-MM-dd} {session.BeginDateTimeInCST:HH:mm} -- {session.EndDateTimeInCST:HH:mm}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"**{session.Speaker}**");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(session.Description);
        if (!string.IsNullOrEmpty(session.DescriptionInZh))
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(session.DescriptionInZh);
        }
        
        return stringBuilder.ToString();
    }
}

file sealed class TranslationHelper
{
    private readonly HttpClient _httpClient;

    public TranslationHelper()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com")
        };
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key",
            Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_API_KEY"));
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Region",
            "southeastasia");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="toLanguage">
    ///  en: english
    ///  zh-Hans: 简体中文
    /// </param>
    /// <returns></returns>
    public async Task<string> GetTranslation(string text, string toLanguage = "zh-Hans")
    {
        using var response = await _httpClient.PostAsJsonAsync($"/translate?api-version=3.0&to={toLanguage}", new[]
        {
            new
            {
                text
            }
        });
        response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<TranslationResponseModel[]>();
        var translatedText = model?.FirstOrDefault()?.Translations.FirstOrDefault()?.Text;
        ArgumentNullException.ThrowIfNull(translatedText);
        return translatedText;
    }
}

file sealed class TranslationResponseModel
{
    public required TranslationLanguageResponseModel[] Translations { get; set; }
}

file sealed class TranslationLanguageResponseModel
{
    public required string To { get; set; }
    public required string Text { get; set; }
}
