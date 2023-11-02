using AngleSharp.Html.Parser;
using System.Text.Encodings.Web;
using System.Text.Json;
using WeihanLi.Common.Helpers;

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
            };
            var dateTitleElements = groupElement.QuerySelectorAll("p.agenda-group-title");
            foreach (var sessionElement in groupElement.QuerySelectorAll(".agenda-group-sessions-container"))
            {
                var session = new SessionModel()
                {
                    SessionId = Convert.ToInt32(sessionElement.GetAttribute("data-sessionid")),
                    DateTimeInfo = dateTitleElements[group.Sessions.Count].TextContent.Trim(),
                    Title = sessionElement.QuerySelector(".agenda-title")?.TextContent.Trim() ?? string.Empty,
                    Speaker = sessionElement.QuerySelector(".agenda-speaker-name")?.TextContent.Trim() ?? string.Empty,
                    Description = sessionElement.QuerySelector(".agenda-description")?.TextContent.Trim() ?? string.Empty,
                };
                group.Sessions.Add(session);
            }
            
            groups.Add(group);
        }

        var groupsJson = JsonSerializer.Serialize(groups, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });
        // Console.WriteLine(groupsJson);

        foreach (var group in groups)
        {
            Console.WriteLine(group.GroupTitle);
            foreach (var session in group.Sessions)
            {
                Console.WriteLine(session.DateTimeInfo);
                Console.WriteLine(session.SessionId);
                Console.WriteLine(session.Title);
                Console.WriteLine(session.Speaker);
                Console.WriteLine(session.Description);
                Console.WriteLine();
            }
        }
    }
}

file sealed class GroupModel
{
    public required int Day { get; set; }
    public required string GroupTitle { get; set; }
    public List<SessionModel> Sessions { get; } = new();
}

file sealed class SessionModel
{
    public required int SessionId { get; set; }
    public required string DateTimeInfo { get; set; }
    public required string Title { get; set; }
    public required string Speaker { get; set; }
    public required string Description { get; set; }
}
