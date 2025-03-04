using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WeihanLi.Extensions;
using WeihanLi.Npoi;
using WeihanLi.Npoi.Attributes;

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

        const string categoryUrlPrefix = "https://www.iherb.com/c/";
        const string productUrlSuffix = "https://www.iherb.com/pr/";

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
            .Where(x => x[categoryMatchedPropName]?.Value<bool>() is true)
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
                if (article.HtmlContent.IsNullOrEmpty())
                    return resultList;
                
                var h2Nodes = doc.QuerySelectorAll("h2");
                if (h2Nodes is not { Length: > 0 })
                    return resultList;
                
                Debug.Assert(doc.Body?.InnerHtml is not null);
                var chunkedContentList = new List<ChunkArticleModel>();
                
                var lastHtmlIdx = doc.Body.InnerHtml.IndexOf(h2Nodes[0].OuterHtml, StringComparison.Ordinal);
                
                for (var i = 1; i < h2Nodes.Length; i++)
                {
                    var htmlIdx = doc.Body.InnerHtml.IndexOf(h2Nodes[i].OuterHtml, StringComparison.Ordinal);
                    if (htmlIdx <= lastHtmlIdx) continue;
                    
                    var htmlChunk = doc.Body.InnerHtml.Substring(lastHtmlIdx, htmlIdx - lastHtmlIdx);
                    var chunk = ExtractChunk(htmlChunk);
                    if (chunk is null) break;
                    
                    chunkedContentList.Add(chunk);

                    lastHtmlIdx = htmlIdx;
                }

                if (lastHtmlIdx < article.TextContent.Length)
                {
                    var chunk = ExtractChunk(doc.Body.InnerHtml.Substring(lastHtmlIdx));
                    if (chunk is not null)
                    {
                        chunkedContentList.Add(chunk);
                    }
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
                            ChunkContent = chunk.ChunkText,
                            ProductIds = products.Where(p => p.Categories.Contains(category))
                                .Select(p => p.Id).ToArray(),
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
                                in_blog_prd = chunk.ProductLinks,
                                in_blog_cat = chunk.CategoryLinks
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
        blogArticles[0].ChunkContent = "test";
        await File.WriteAllTextAsync(articlesJsonPath.Replace(".json", ".result-sample.json"), blogArticles[0].ToIndentedJson());
        Console.WriteLine("Completed");
        
        
        static ChunkArticleModel? ExtractChunk(string? htmlChunk)
        {
            if(htmlChunk.IsNullOrWhiteSpace()) return null;
            
            var htmlChunkDoc = HtmlParser.ParseDocument(htmlChunk);
            if (htmlChunkDoc?.Body?.TextContent is null) return null;
                    
            var links = htmlChunkDoc.QuerySelectorAll("a")
                .Select(link => link.GetAttribute("href"))
                .WhereNotNull()
                .Where(href => !string.IsNullOrEmpty(href) 
                               && (
                                   href.StartsWith(categoryUrlPrefix) ||
                                   href.StartsWith(productUrlSuffix)
                               )
                )
                .ToArray();
            var chunk = new ChunkArticleModel()
            {
                ChunkText = htmlChunkDoc.Body?.TextContent!,
                CategoryLinks = links.Where(x=> x.StartsWith(categoryUrlPrefix)).ToArray(),
                ProductLinks = links.Where(x => x.StartsWith(productUrlSuffix)).ToArray()
            };

            if (links.Length > 0)
            {
                chunk.ChunkText = $"{chunk.ChunkText}  {string.Join("  ", links)}";
            }
            
            return chunk;
        }
    }

    public static async Task AnalyzeProducts()
    {
        const string newArticlesJsonPath = @"/Users/weihan.li/repositories/ConsoleTestApp/ConsoleTestApp/bin/Debug/net8.0/articles.result.json";
        if (!File.Exists(newArticlesJsonPath)) throw new InvalidOperationException("");
        
        var articlesText = await File.ReadAllTextAsync(newArticlesJsonPath);
        var articles = JsonConvert.DeserializeObject<ResultModel[]>(articlesText);
       ArgumentNullException.ThrowIfNull(articles);
        var productIds = articles
                .SelectMany(x => x.ProductIds ?? [])
                .ToHashSet()
            ;
        var productIdsJson = productIds.ToJson();
        Console.WriteLine(productIdsJson);
    }

    public static async Task ExternalArticlesProcessing()
    {
        const string articlesJsonPath = @"/Users/weihan.li/repositories/ConsoleTestApp/ConsoleTestApp/bin/Debug/net8.0/articles.external.json";
        const string ExternalArticlePath = "/Users/weihan.li/Downloads/ExternalArticles.xlsx";
        var articles = ExcelHelper.ToEntities<BlogArticle>(ExternalArticlePath, 1)
            .Where(a => a is { ArticleId: > 0 })
            .ToArray()
            ;
        ArgumentNullException.ThrowIfNull(articles);

        
        var resultList = new List<ResultModel>();
        
        foreach (var article in articles)
        {
            Debug.Assert(article?.Products is not null);
            var products = article.Products.JsonToObject<BlogProduct[]>();
            var categories = products.SelectMany(p => p.Categories).ToHashSet();

            Debug.Assert(article.TextContent is not null);
            var chunks = article.TextContent.Split("###", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var category in categories)
            {
                foreach (var chunk in chunks)
                {
                    var resultModel = new ResultModel
                    {
                        ChunkType = "external",
                        Label = category,
                        ArticleId = article.ArticleId,
                        ArticlePath = article.ArticlePath,
                        Title = article.Title,
                        Subtitle = article.Subtitle,
                        TextContent = article.TextContent,
                        ChunkContent = chunk,
                        ProductIds = products.Where(p => p.Categories.Contains(category))
                            .Select(p => p.Id).ToArray(),
                        AdditionalInfo = new
                        {
                            content_id = article.ArticleId.ToString(),
                            content_type = "external",
                            products_info = products.Select(p => new { product_id = p.Id, product_name = p.Name }),
                            article_path = article.ArticlePath,
                            title = article.Title,
                            subtitle = article.Subtitle,
                        }.ToJson()
                    };
                    resultList.Add(resultModel);
                }
            }
        }
        
        var blogArticlesJsonText = JsonConvert.SerializeObject(resultList);
        await File.WriteAllTextAsync(articlesJsonPath, blogArticlesJsonText);

        resultList[0].TextContent = "test";
        await File.WriteAllTextAsync(articlesJsonPath.Replace(".json", ".sample.json"), resultList[0].ToIndentedJson());
    }

    private static readonly HtmlParser HtmlParser = new();
    
}

file sealed class ExternalArticles
{
    
}

sealed class ChunkArticleModel
{
    public string ChunkText { get; set; }
    public string[] ProductLinks { get; set; }
    public string[] CategoryLinks { get; set; }
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
    public string ArticlePath { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    [JsonIgnore]
    [Column(IsIgnored = true)]
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
    public required int[] ProductIds { get; set; }
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
