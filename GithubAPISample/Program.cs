// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using System.Text.Json.Nodes;
using WeihanLi.Extensions;
using WeihanLi.Npoi;

const int pageCount = 30;
const string userName = "weihanli";
const string urlFormat =
    $"search/issues?page={{0}}&q=state%3Aclosed+author%3A{userName}+type%3Apr+is:merged+merged:%3E=2021-01-01";

var prList = new List<GithubPRModel>();
var itemsCount = 0;
var pageNum = 1;

using var httpClient = new HttpClient()
{
    BaseAddress = new Uri("https://api.github.com/")
};
// https://docs.github.com/en/rest/overview/resources-in-the-rest-api#user-agent-required
httpClient.DefaultRequestHeaders
    .TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36");

do
{
    var url = urlFormat.FormatWith(pageNum.ToString());
    //using var response = await httpClient.GetAsync(url);
    //var responseText = await response.Content.ReadAsStringAsync();
    //var responseObj = JsonNode.Parse(responseText);

    var responseObj = await httpClient.GetFromJsonAsync<JsonObject>(url);
    ArgumentNullException.ThrowIfNull(responseObj);
    var items = responseObj["items"]?.AsArray();
    ArgumentNullException.ThrowIfNull(items);
    itemsCount = items.Count;

    foreach (var item in items)
    {
        prList.Add(new GithubPRModel()
        {
            Title = item["title"]?.GetValue<string>(),
            Body = item["body"]?.GetValue<string>(),
            Url = item["html_url"]?.GetValue<string>(),
            CreatedAt = item["created_at"]?.GetValue<DateTimeOffset?>(),
            ClosedAt = item["closed_at"]?.GetValue<DateTimeOffset>()
        });
    }

    pageNum++;
} while (itemsCount == pageCount);

var totalCount = prList.Count;
prList.RemoveAll(x => userName.Equals(x.UserName, StringComparison.OrdinalIgnoreCase));
var excludedCount = prList.Count;
Console.WriteLine($"Total:{totalCount}, excludedCount: {excludedCount}");

prList.OrderBy(x => x.RepoName)
    .ThenBy(x => x.CreatedAt)
    .ToCsvFile(Path.Combine(Directory.GetCurrentDirectory(), "pr-list.csv"));

var result = prList.GroupBy(x => x.RepoName)
    .Select(g => $@"{g.Key}
{g.OrderBy(x => x.CreatedAt).Select(x => $"\t{x.Title}\t{x.CreatedAt}\t{x.ClosedAt}").StringJoin(Environment.NewLine)}
").StringJoin(Environment.NewLine);

Console.WriteLine(result);
Console.ReadLine();

internal class GithubPRModel
{
    private string _url;
    public string Title { get; set; }
    public string Body { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    public string Url
    {
        get => _url;
        set
        {
            _url = value;
            if (!string.IsNullOrEmpty(_url))
            {
                var index = _url.IndexOf("/pull/", StringComparison.OrdinalIgnoreCase);
                RepoUrl = _url[..index];
                RepoName = RepoUrl.Replace("https://github.com/", "");
                UserName = RepoName[..RepoName.IndexOf('/')];
            }
        }
    }

    public string UserName { get; set; }

    public string RepoName { get; private set; }

    public string RepoUrl { get; private set; }
}
