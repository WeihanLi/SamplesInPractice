using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeihanLi.Extensions;

namespace BalabalaSample;

public class BlogProductSample
{
    public static async Task RunAsync()
    {
        // const string articlesJsonPath = @"C:\Users\Weiha\Desktop\articles.json";
        // const string newArticlesJsonPath = @"C:\Users\Weiha\Desktop\articles.new.json";
        // const string productCategoriesJsonPath = @"C:\Users\Weiha\Desktop\All_category_for_demo.txt";

        const string articlesJsonPath = @"/Users/weihan.li/repositories/ConsoleTestApp/ConsoleTestApp/bin/Debug/net8.0/articles.json";
        const string newArticlesJsonPath = @"/Users/weihan.li/repositories/ConsoleTestApp/ConsoleTestApp/bin/Debug/net8.0/articles.new.json";
        const string productCategoriesJsonPath = @"/Users/weihan.li/repositories/ConsoleTestApp/ConsoleTestApp/bin/Debug/net8.0/All_category_for_demo.txt";
        const string categoryMatchedPropName = "CategoryMatched";

        JArray articles;
        if (File.Exists(newArticlesJsonPath))
        {
            var articlesText = await File.ReadAllTextAsync(newArticlesJsonPath);
            articles = JArray.Parse(articlesText);
        }
        else
        {
            var articlesText = await File.ReadAllTextAsync(articlesJsonPath);
            var categoriesText = await File.ReadAllTextAsync(productCategoriesJsonPath);
            var categories = categoriesText.JsonToObject<HashSet<string>>();

            articles = JArray.Parse(articlesText);
            foreach (var article in articles)
            {
                var recommendProducts = article["RecommendProducts"]?.Value<string>()?.JsonToObject<List<JObject>>();
                if (recommendProducts is not { Count: > 0 })
                {
                    continue;
                }

                for (var i = recommendProducts.Count - 1; i >= 0; i--)
                {
                    var product = recommendProducts[i]["More"]?.Value<string>();
                    ArgumentNullException.ThrowIfNull(product);
                    var categoryNames = new List<string>();

                    var productObject = JObject.Parse(product);
                    var rootCategoryName = productObject["RootCategoryName"]?.Value<string>();
                    if (rootCategoryName.IsNotNullOrEmpty())
                    {
                        categoryNames.Add(rootCategoryName);
                    }

                    var rankCategoryNames = productObject["ProductRanks"]?.Select(r =>
                        r["CategoryDisplayName"]?.Value<string>()).WhereNotNull().ToArray() ?? [];
                    foreach (var name in rankCategoryNames)
                    {
                        categoryNames.Add(name);
                    }

                    var canonicalNames = productObject["CanonicalPaths"]?.SelectMany(p => p)
                        .Select(p => p["DisplayName"]?.Value<string>())
                        .WhereNotNull() ?? [];
                    foreach (var name in canonicalNames)
                    {
                        categoryNames.Add(name);
                    }

                    var matchedCategories = categoryNames.Where(n => categories.Contains(n))
                        .ToArray();
                    if (matchedCategories.Length > 0)
                    {
                        productObject[categoryMatchedPropName] = JToken.FromObject(matchedCategories);
                    }

                    recommendProducts[i]["More"] = JToken.FromObject(productObject);
                }

                recommendProducts.RemoveAll(p => p["More"]?[categoryMatchedPropName]?.ToObject<string[]>()
                    is not { Length: > 0 });

                article[categoryMatchedPropName] = recommendProducts is { Count: > 0 };
                article["RecommendProducts"] = JToken.FromObject(recommendProducts);
            }

            var newArticlesText = JsonConvert.SerializeObject(articles);
            await File.WriteAllTextAsync(newArticlesJsonPath, newArticlesText);
        }

        //
        var blogArticles = articles
            .Where(x => x["CategoryMatched"]?.Value<bool>() is true)
            .SelectMany(a =>
            {
                var products = a["RecommendProducts"]!.Select(p => new BlogProduct
                {
                    Id = p["Id"]!.Value<int>(),
                    Name = p["Name"]!.Value<string>()!,
                    Categories =
                        p["More"]!["CategoryMatched"]?.ToObject<HashSet<string>>() ?? new()
                }).Where(p => p.Categories is { Count: > 0 }).ToArray();
                var article = new BlogArticle
                {
                    ArticleId = a["ArticleId"]!.Value<int>(),
                    ArticlePath = a["ArticlePath"]!.Value<string>()!,
                    Title = a["Title"]!.Value<string>(),
                    Subtitle = a["Subtitle"]!.Value<string>(),
                    HtmlContent = a["HtmlContent"]!.Value<string>(),
                    TextContent = a["TextContent"]!.Value<string>(),
                    Products = products.ToJson()
                };
                
                var categories = products.SelectMany(p => p.Categories).ToHashSet();
                var resultList = new List<ResultModel>();
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(article.HtmlContent ?? string.Empty);
                if (article.TextContent.IsNullOrEmpty())
                {
                    article.TextContent = doc.Body?.TextContent ?? string.Empty;
                }
                if (article.TextContent.IsNullOrEmpty())
                    return resultList;
                Debug.Assert(article.TextContent is not null);
                
                var h2Nodes = doc.QuerySelectorAll("h2");
                var chunkedContentList = new List<string>();
                var lastIndex = article.TextContent.IndexOf(h2Nodes[0].TextContent, StringComparison.OrdinalIgnoreCase);;
                for (var i = 1; i < h2Nodes.Length; i++)
                {
                    var nextTitle = h2Nodes[i].TextContent;
                    var idx = article.TextContent.IndexOf(nextTitle, StringComparison.OrdinalIgnoreCase);
                    if (idx > lastIndex)
                    {
                        var chunk = article.TextContent!.Substring(lastIndex, idx - lastIndex);
                        chunkedContentList.Add(chunk);
                    }

                    lastIndex = idx;
                }

                if (lastIndex < article.TextContent.Length)
                {
                    chunkedContentList.Add(article.TextContent.Substring(lastIndex));
                }

                foreach (var category in categories)
                {
                    foreach (var chunk in chunkedContentList)
                    {
                        var model = new ResultModel
                        {
                            Label = category,
                            ArticleId = article.ArticleId,
                            ArticlePath = article.ArticlePath,
                            Title = article.Title,
                            Subtitle = article.Subtitle,
                            TextContent = article.TextContent,
                            ChunkContent = chunk,
                            ProductIds = products.Where(p => p.Categories.Contains(category))
                                .Select(p => p.Id).ToJson(),
                            AdditionalInfo = new
                            {
                                content_id = article.ArticleId.ToString(),
                                content_type = "blog",
                                products_info = products.Select(p => new
                                {
                                    product_id = p.Id,
                                    product_name = p.Name
                                }),
                                article_path = article.ArticlePath,
                                title = article.Title,
                                subtitle = article.Subtitle,
                            }.ToJson()
                        };
                        resultList.Add(model);
                    }
                }

                return resultList;
            }).ToArray();

        var blogArticlesJsonText = JsonConvert.SerializeObject(blogArticles);
        await File.WriteAllTextAsync(articlesJsonPath.Replace(".json", ".result.json"), blogArticlesJsonText);

        blogArticles[0].TextContent = "test";
        await File.WriteAllTextAsync(articlesJsonPath.Replace(".json", ".result-sample.json"), blogArticles[0].ToIndentedJson());
    }
}

sealed class BlogProduct
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required HashSet<string> Categories { get; set; }
}

sealed record BlogArticle
{
    public int ArticleId { get; set; }
    public required string ArticlePath { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    [JsonIgnore]
    public string? HtmlContent { get; set; }
    public string? TextContent { get; set; }
    public string? Products { get; set; }
}

sealed class ResultModel
{
    [JsonProperty("language_code")]
    public string LanguageCode { get; set; } = "en-US";

    [JsonProperty("chunk_type")]
    public string ChunkType { get; set; } = "blog";
    
    [JsonProperty("label")]
    public required string Label { get; set; }

    [JsonProperty("product_ids")]
    public required string ProductIds { get; set; }
    [JsonIgnore]
    public int ArticleId { get; set; }
    [JsonIgnore]
    public required string ArticlePath { get; set; }
    [JsonIgnore]
    public string? Title { get; set; }
    [JsonIgnore]
    public string? Subtitle { get; set; }
    
    [JsonProperty("full_text")]
    public string? TextContent { get; set; }
    
    [JsonProperty("chunk_text")]
    public string? ChunkContent { get; set; }

    [JsonProperty("additional_info")]
    public string AdditionalInfo { get; set; }
}
