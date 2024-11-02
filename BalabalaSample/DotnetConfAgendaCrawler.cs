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
        var pstTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        var agendaHtml = await HttpHelper.HttpClient.GetStringAsync("https://dotnetconf.com/agenda");
        var htmlParser = new HtmlParser();
        var doc = await htmlParser.ParseDocumentAsync(agendaHtml);

        var agendaContainerElements = doc.QuerySelectorAll(".agenda-container");
        ArgumentNullException.ThrowIfNull(agendaContainerElements);
        var groups = new List<GroupModel>();

        foreach (var agendaContainerElement in agendaContainerElements)
        {
            var dayAttributeName = "data-day";
            var sessionIdAttributeName = "data-sessionid";
            var dataStartAttributeName = "data-start";
            var dataEndAttributeName = "data-end";
            var dayGroupElements = agendaContainerElement.QuerySelectorAll("div[data-day]");
            if (dayGroupElements.Length is 0)
            {
                dayAttributeName = "data-bonusday";
                sessionIdAttributeName = "data-bonussessionid";
                dataStartAttributeName = "data-bonusstart";
                dataEndAttributeName = "data-bonusend";
                dayGroupElements = agendaContainerElement.QuerySelectorAll("div[data-bonusdate]");
            }
            var subGroups = new List<GroupModel>();
            foreach (var groupElement in agendaContainerElement.QuerySelectorAll(".agenda-group"))
            {
                var dayGroupElement = dayGroupElements[subGroups.Count];
                var day = Convert.ToInt32(dayGroupElement.GetAttribute(dayAttributeName));
                var groupTitle = dayGroupElement.TextContent.Trim()
                        .Replace("\r\n", "\n")
                        .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .StringJoin(" - ");
                var group = new GroupModel()
                {
                    Day = day,
                    GroupTitle = groupTitle
                };
                var dateTitleElements = groupElement.QuerySelectorAll("p.agenda-group-title");
                foreach (var sessionElement in groupElement.QuerySelectorAll(".agenda-group-sessions-container"))
                {
                    var dateElement = dateTitleElements[group.Sessions.Count].QuerySelector($"span[{dataStartAttributeName}]");
                    var startDateString = dateElement?.GetAttribute(dataStartAttributeName);
                    ArgumentNullException.ThrowIfNull(startDateString);
                    var endDateString = dateElement?.GetAttribute(dataEndAttributeName);
                    ArgumentNullException.ThrowIfNull(endDateString);
                    var pstStartDate = DateTime.Parse(startDateString.Replace("PST", "").Trim());
                    var startDate = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(pstStartDate, pstTimeZoneInfo), TimeSpan.Zero);
                    var pstEndDate = DateTime.Parse(endDateString.Replace("PST", "").Trim());
                    var endDate = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(pstEndDate, pstTimeZoneInfo), TimeSpan.Zero);
                    var session = new SessionModel()
                    {
                        SessionId = Convert.ToInt32(sessionElement.GetAttribute(sessionIdAttributeName)),
                        BeginDateTime = startDate,
                        EndDateTime = endDate,
                        Title = sessionElement.QuerySelector(".agenda-title")?.TextContent.Trim() ?? string.Empty,
                        Speaker =
                            sessionElement.QuerySelector(".agenda-speaker-name")?.TextContent.Trim() ?? string.Empty,
                        Description = sessionElement.QuerySelector(".agenda-description")?.TextContent.Trim() ??
                                      string.Empty,
                    };
                    group.Sessions.Add(session);
                }

                group.Sessions.Sort();
                subGroups.Add(group);
            }
            
            groups.AddRange(subGroups);
        }


        if (groups.Count == 0)
        {
            Console.WriteLine("Nothing found");
            return;
        }

        var sessions = groups.SelectMany(s => s.Sessions)
            .OrderBy(x => x.BeginDateTimeInCST).ThenBy(x => x.SessionId)
            .ToList();

        var translationHelper = new TranslationHelper();
        if (translationHelper.ApiKeyConfigured)
        {
            foreach (var session in sessions)
            {
                try
                {
                    await RetryHelper.TryInvokeAsync(
                        async () =>
                        {
                            session.DescriptionInZh = await translationHelper.GetTranslation(session.Description);
                        }, 5,
                        (i, _, ex) =>
                            ConsoleHelper.WriteLineWithColor($"Exception, retry count: {i}, {ex}", ConsoleColor.Red),
                        delayFunc: c => TimeSpan.FromSeconds(c * 5));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        var sessionsJson = JsonConvert.SerializeObject(sessions, Formatting.Indented);
        Console.WriteLine(sessionsJson);
        var year = DateTimeOffset.Now.Year.ToString();
        await File.WriteAllTextAsync($"dotnetconf{year}-agenda.json", sessionsJson);
        //
        var markdownSnippets = string.Join(Environment.NewLine, sessions.Select(x => x.GetMarkdownSnippet()));
        Console.WriteLine(markdownSnippets);
        await File.WriteAllTextAsync($"dotnetconf{year}-agenda.md", markdownSnippets);
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
        foreach (var session in Sessions.OrderBy(x => x.BeginDateTime))
        {
            stringBuilder.AppendLine(session.GetMarkdownSnippet());
            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }
}

file sealed class SessionModel : IComparable, IComparable<SessionModel>
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

    public bool ShouldSerializeDescriptionInZh() => !string.IsNullOrEmpty(DescriptionInZh);

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
        stringBuilder.AppendLine(
            $"> {session.BeginDateTimeInCST.Date:yyyy-MM-dd} {session.BeginDateTimeInCST:HH:mm} -- {session.EndDateTimeInCST:HH:mm}");
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

// https://learn.microsoft.com/zh-cn/azure/ai-services/translator/reference/v3-0-translate
file sealed class TranslationHelper
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public TranslationHelper()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com") };
        _apiKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_API_KEY");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", _apiKey);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Region", "southeastasia");
    }

    public bool ApiKeyConfigured => !string.IsNullOrEmpty(_apiKey);

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
        text = text.Trim();
        if (string.IsNullOrEmpty(text)) return string.Empty;

        using var response =
            await _httpClient.PostAsJsonAsync($"/translate?api-version=3.0&to={toLanguage}", new[] { new { text } });
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
