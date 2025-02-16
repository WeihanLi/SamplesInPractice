using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeihanLi.Extensions;
using WeihanLi.Npoi;

namespace BalabalaSample;

public class BlogProductSample
{
    public static async Task RunAsync()
    {
        const string articlesJsonPath = @"C:\Users\Weiha\Desktop\articles.json";
        const string newArticlesJsonPath = @"C:\Users\Weiha\Desktop\articles.new.json";
        const string productCategoriesJsonPath = @"C:\Users\Weiha\Desktop\All_category_for_demo.txt";
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
        var blogArticles = articles.Where(x => x["CategoryMatched"]?.Value<bool>() is true)
            .Select(a => new BlogArticle
            {
                ArticleId = a["ArticleId"]!.Value<int>(),
                ArticlePath = a["ArticlePath"]!.Value<string>()!,
                Title = a["Title"]!.Value<string>(),
                Subtitle = a["Subtitle"]!.Value<string>(),
                TextContent = a["TextContent"]!.Value<string>(),
                Products = a["RecommendProducts"]!.Select(p => new BlogProduct
                {
                    Id = p["Id"]!.Value<int>(),
                    Name = p["Name"]!.Value<string>()!,
                    Categories =
                        p["More"]!["CategoryMatched"]?.ToObject<string[]>()?.StringJoin(";") ?? string.Empty
                }).Where(p => !string.IsNullOrEmpty(p.Categories)).ToJson()
            }).ToArray();
        var blogArticlesJsonText = JsonConvert.SerializeObject(blogArticles);
        await File.WriteAllTextAsync(articlesJsonPath.Replace(".json", ".normalized.json"), blogArticlesJsonText);
        //
        await blogArticles.ToCsvFileAsync(articlesJsonPath.Replace(".json", ".csv"));
    }
}

file sealed class BlogProduct
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Categories { get; set; }
}

file sealed class BlogArticle
{
    public int ArticleId { get; set; }
    public required string ArticlePath { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? TextContent { get; set; }
    public string? Products { get; set; }
}
