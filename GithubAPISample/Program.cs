// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using System.Text.Json.Nodes;
using WeihanLi.Extensions;
using WeihanLi.Npoi;

const int pageCount = 30;
const string userName = "weihanli";

var year = DateTimeOffset.Now.Year;
var beginDate = $"{year-1}-04-01";
var endDate = $"{year}-03-31";
var urlFormat = $"search/issues?page={{0}}&q=author%3A{userName}+type%3Apr+is:merged+merged:{beginDate}..{endDate}";

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
            CreatedAt = item["created_at"]?.GetValue<DateTimeOffset>(),
            ClosedAt = item["closed_at"]?.GetValue<DateTimeOffset>()
        });
    }

    pageNum++;
} while (itemsCount == pageCount);

var totalCount = prList.Count;
prList.RemoveAll(x => userName.Equals(x.UserName, StringComparison.OrdinalIgnoreCase));
var excludedCount = prList.Count;
var repoCount = prList.GroupBy(x => x.RepoUrl).Count();
Console.WriteLine($"Total PR: {totalCount}, externalContributionCount: {excludedCount}, external repository count: {repoCount}");

prList.OrderBy(x => x.RepoName)
    .ThenBy(x => x.CreatedAt)
    .ToCsvFile(Path.Combine(Directory.GetCurrentDirectory(), $"pr-list-{year}.csv"));

// var result = prList.GroupBy(x => x.RepoName)
//     .Select(g => $@"{g.Key}
// {g.OrderBy(x => x.CreatedAt).Select(x => $"\t{x.Title}\t{x.CreatedAt}\t{x.ClosedAt}").StringJoin(Environment.NewLine)}
// ").StringJoin(Environment.NewLine);

var mdContent = $"""
# {year-1} - {year} PR Contributions

## Intro

{beginDate} - {endDate}（按 PR 合并的时间），共 {totalCount} 个 PR，其中 {excludedCount} 个外部贡献，涉及 {repoCount} 个仓库。

## All PRs

{prList.GroupBy(g => new
{
  g.RepoName,
  g.RepoUrl
})
.OrderBy(g => g.Key.RepoName)
.Select(g => $@"- [{g.Key.RepoName}]({g.Key.RepoUrl})
{g.OrderBy(x => x.CreatedAt).Select(x => $"  - {x.Title}({x.ClosedAt:yyyy-MM-dd}) <{x.Url}>").StringJoin(Environment.NewLine)}
")
.StringJoin(Environment.NewLine)}

## More

以上列表通过 Github 的 Rest API 获取，具体源代码在 Github 上，可以参考：
<https://github.com/WeihanLi/SamplesInPractice/blob/main/GithubAPISample/Program.cs>

""";

await File.WriteAllTextAsync($"result-{year}.md", mdContent);
Console.WriteLine("Completed");
Console.ReadLine();

file sealed class GithubPRModel
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
