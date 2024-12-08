# 使用 nuget server 的 API 来实现搜索安装 nuget 包

## Intro

nuget 现在几乎是 dotnet 开发不可缺少的一部分了，还没有用过 nuget 的就有点落后时代了，还不快用起来

nuget 是 dotnet 里的包管理机制，类似于前端的 npm ，php 的 composer，java 里的 maven ...

nuget 定义了一套关于 nuget server 的规范，使得用户可以自己实现一个 nuget server

也正是这些规范，使得我们可以根据这些规范来实现 nuget server 的包管理的功能，今天主要介绍一下，根据 nuget server 的 api 规范使用原始的 HTTP 请求来实现 nuget 包的搜索和使用 nuget 提供的客户端 SDK 来实现 nuget 包的搜索和下载

## Nuget Server Api

### Nuget 协议介绍

nuget 的协议有好几个版本，目前主要用的是 v3，开源的 nuget server Baget 也实现了基于 nuget protocal v3 的规范

我们添加 nuget 源的时候会指定一个 source url，类似 `https://api.nuget.org/v3/index.json` 这样的，着通常被称为 Service Index，是一个 nuget 源的入口，有点类似于 Identity Server 里的发现文档，通过这个地址可以获取到一系列的资源的地址

有一些资源是协议规范里定义的必须要实现的，有一些是可选的，具体参考官方文档，以后随着版本变化，可能会有差异，目前 nuget.org 提供的资源如下：

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627111811819-279166472.png)


Nuget.org 提供了两种搜索的方式，

一个是 SearchQuery，会根据包名称、 tag、description 等信息去匹配关键词，

一个是 SearchAutocomplete 根据包名称的前缀去匹配包的名称

获取某个 nuget 包的版本信息，可以使用 PackageBaseAddress 来获取



`ServiceIndex` 返回的信息示例如下：

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627111829479-1461215919.png)

返回的信息会有一个 `resources` 的数组，会包含各种不同类型的资源，对应的 `@id` 就是调用这种类型的API要用到的地址，下面来看一个搜索的示例

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627111854100-624274919.png)

在每个 API 的文档页面可以看到会使用的 `@type`，调用这个 API 的时候应该使用这些 `@type` 对应的资源

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627111921656-665319921.png)


这里的 `@id` 就是上面的 resource 对应的 `@id`，

参数说明：

`q` 搜索时所用的关键词，

`skip`/`take` 用来分页显示查询结果，

 `prelease` 用来指定是否限制预览版的 package，`true` 包含预览版的 nuget 包，`false` 只包含已经正式发布的 nuget 包

`semVerLevel` 是用来指定包的语义版本

> The `semVerLevel` query parameter is used to opt-in to [SemVer 2.0.0 packages](https://github.com/NuGet/Home/wiki/SemVer2-support-for-nuget.org-(server-side)#identifying-semver-v200-packages). If this query parameter is excluded, only packages with SemVer 1.0.0 compatible versions will be returned (with the [standard NuGet versioning](https://docs.microsoft.com/en-us/nuget/concepts/package-versioning) caveats, such as version strings with 4 integer pieces). If `semVerLevel=2.0.0` is provided, both SemVer 1.0.0 and SemVer 2.0.0 compatible packages will be returned. See the [SemVer 2.0.0 support for nuget.org](https://github.com/NuGet/Home/wiki/SemVer2-support-for-nuget.org-(server-side)) for more information

`packageType` 用来指定 nuget 包的类型，目前支持的类型包括 `Dependency`(默认)项目依赖项，`DotnetTool`(dotnetcore 2.1 引入的 dotnet cli tool)，`Template` (dotnet new 用) 自定义的项目模板



其他的 API 可以自行参考官方文档：<https://docs.microsoft.com/en-us/nuget/api/service-index>

### Packages

`SearchQuery` 返回的信息比较多而且可能并不准确，适用于不清楚包的名称的时候使用，如果知道 nuget 包的名称（PackageId） ，可以使用 `SearchAutocomplete` 来搜索，这样更精准，返回的信息也更简单，只有匹配的 package 名称

通过原始 api 调用的方式实现 nuget 包的搜索

``` csharp
using var httpClient = new HttpClient(new NoProxyHttpClientHandler());
// loadServiceIndex
var serviceIndexResponse = await httpClient.GetStringAsync(NugetServiceIndex);
var serviceIndexObject = JObject.Parse(serviceIndexResponse);

var keyword = "weihanli";

//https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource
var queryEndpoint = serviceIndexObject["resources"]
    .First(x => x["@type"].Value<string>() == "SearchQueryService")["@id"]
    .Value<string>();
var queryUrl = $"{queryEndpoint}?q={keyword}&skip=0&take=5&prerelease=false&semVerLevel=2.0.0";
var queryResponse = await httpClient.GetStringAsync(queryUrl);
Console.WriteLine($"formatted queryResponse:");
Console.WriteLine($"{JObject.Parse(queryResponse).ToString(Formatting.Indented)}");

// https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource
var autoCompleteQueryEndpoint = serviceIndexObject["resources"]
    .First(x => x["@type"].Value<string>() == "SearchAutocompleteService")["@id"]
    .Value<string>();
var autoCompleteQueryUrl = $"{autoCompleteQueryEndpoint}?q={keyword}&skip=0&take=5&prerelease=false&semVerLevel=2.0.0";
var autoCompleteQueryResponse = await httpClient.GetStringAsync(autoCompleteQueryUrl);
Console.WriteLine($"formatted autoCompleteQueryResponse:");
Console.WriteLine($"{JObject.Parse(autoCompleteQueryResponse).ToString(Formatting.Indented)}");

```

output 示例：

Query 返回示例

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627112006959-762099850.png)

Autocomplete 返回结果

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627112034651-426780939.png)

从上面我们可以看到 Query 接口返回了很多的信息，Autocomplete 接口只返回了 package 的名称，返回的信息更为简洁，所以如果可以使用 Autocomplete 的方式就尽可能使用 Autocomplete 的方式

### Package Versions

前面我们提到了可以使用 `PackageBaseAddress` 来查询某个 nuget 包的版本信息，文档地址：<https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource>，来看一下示例：

``` csharp
using (var httpClient = new HttpClient(new NoProxyHttpClientHandler()))
{
    // loadServiceIndex
    var serviceIndexResponse = await httpClient.GetStringAsync(NugetServiceIndex);
    var serviceIndexObject = JObject.Parse(serviceIndexResponse);

    // https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource
    var packageVersionsEndpoint = serviceIndexObject["resources"]
        .First(x => x["@type"].Value<string>() == "PackageBaseAddress/3.0.0")["@id"]
        .Value<string>();

    var packageVersionsQueryUrl = $"{packageVersionsEndpoint}/dbtool.core/index.json";
    var packageVersionsQueryResponse = await httpClient.GetStringAsync(packageVersionsQueryUrl);
    Console.WriteLine("DbTool.Core versions:");
    Console.WriteLine(JObject.Parse(packageVersionsQueryResponse)
                      .ToString(Formatting.Indented));
}
```

output 示例：

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627112102954-342768753.png)

> 注：api 地址中的 packageId 要转小写

## Nuget Client SDK

除了上面的根据 api 自己调用，我们还可以使用 nuget 提供的客户端 sdk 实现上述功能，这里就不详细介绍了，有需要可能查阅官方文档：<https://docs.microsoft.com/en-us/nuget/reference/nuget-client-sdk>

下面给出一个使用示例：

``` csharp
var packageId = "WeihanLi.Common";
var packageVersion = new NuGetVersion("1.0.38");

var logger = NullLogger.Instance;
var cache = new SourceCacheContext();
// 在 SDK 的概念里，每一个 nuget 源是一个 repository
var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

// SearchQuery
{
    var resource = await repository.GetResourceAsync<PackageSearchResource>();
    var searchFilter = new SearchFilter(includePrerelease: false);

    var results = await resource.SearchAsync(
        "weihanli",
        searchFilter,
        skip: 0,
        take: 20,
        logger,
        CancellationToken.None);
    foreach (var result in results)
    {
        Console.WriteLine($"Found package {result.Identity.Id} {result.Identity.Version}");
    }
}
// SearchAutoComplete
{
    var autoCompleteResource = await repository.GetResourceAsync<AutoCompleteResource>();
    var packages =
        await autoCompleteResource.IdStartsWith("WeihanLi", false, logger, CancellationToken.None);
    foreach (var package in packages)
    {
        Console.WriteLine($"Found Package {package}");
    }
}
//
{
    // get package versions
    var findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>();
    var versions = await findPackageByIdResource.GetAllVersionsAsync(
        packageId,
        cache,
        logger,
        CancellationToken.None);

    foreach (var version in versions)
    {
        Console.WriteLine($"Found version {version}");
    }
}
```

## More

你可以使用 nuget sdk 方便的实现 nuget 包的下载安装，内部实现了签名校验等，这样就可以把本地不存在的 nuget 包下载到本地了，

实现示例：

``` csharp
{
    var pkgDownloadContext = new PackageDownloadContext(cache);
    var downloadRes = await repository.GetResourceAsync<DownloadResource>();

    var downloadResult = await RetryHelper.TryInvokeAsync(async () =>
        await downloadRes.GetDownloadResourceResultAsync(
            new PackageIdentity(packageId, packageVersion),
            pkgDownloadContext,
            @"C:\Users\liweihan\.nuget\packages", // nuget globalPackagesFolder
            logger,
            CancellationToken.None), r => true);
    Console.WriteLine(downloadResult.Status.ToString());
}
```

最后提供一个解析 nuget `globalPackagesFolder` 的两种思路：

一个是前面有篇文章介绍的，有个默认的配置文件，然后就是默认的配置，写了一个解析的方法示例，支持 Windows/Linux/Mac：

``` csharp
{
    var packagesFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

    if (string.IsNullOrEmpty(packagesFolder))
    {
        // Nuget globalPackagesFolder resolve
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var defaultConfigFilePath =
                $@"{Environment.GetEnvironmentVariable("APPDATA")}\NuGet\NuGet.Config";
            if (File.Exists(defaultConfigFilePath))
            {
                var doc = new XmlDocument();
                doc.Load(defaultConfigFilePath);
                var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                if (node != null)
                {
                    packagesFolder = node.Attributes["value"]?.Value;
                }
            }

            if (string.IsNullOrEmpty(packagesFolder))
            {
                packagesFolder = $@"{Environment.GetEnvironmentVariable("USERPROFILE")}\.nuget\packages";
            }
        }
        else
        {
            var defaultConfigFilePath =
                $@"{Environment.GetEnvironmentVariable("HOME")}/.config/NuGet/NuGet.Config";
            if (File.Exists(defaultConfigFilePath))
            {
                var doc = new XmlDocument();
                doc.Load(defaultConfigFilePath);
                var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                if (node != null)
                {
                    packagesFolder = node.Attributes["value"]?.Value;
                }
            }

            if (string.IsNullOrEmpty(packagesFolder))
            {
                defaultConfigFilePath = $@"{Environment.GetEnvironmentVariable("HOME")}/.nuget/NuGet/NuGet.Config";
                if (File.Exists(defaultConfigFilePath))
                {
                    var doc = new XmlDocument();
                    doc.Load(defaultConfigFilePath);
                    var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                    if (node != null)
                    {
                        packagesFolder = node.Value;
                    }
                }
            }

            if (string.IsNullOrEmpty(packagesFolder))
            {
                packagesFolder = $@"{Environment.GetEnvironmentVariable("HOME")}/.nuget/packages";
            }
        }
    }

    Console.WriteLine($"globalPackagesFolder: {packagesFolder}");
}
```



另一个是可以根据 nuget 提供的一个命令查询 `nuget locals global-packages -l`，通过命令输出获取

![](https://img2020.cnblogs.com/blog/489462/202006/489462-20200627112133460-1742942992.png)

## Reference

- <https://github.com/WeihanLi/SamplesInPractice/blob/master/NugetSample/RawApiSample.cs>
- <https://github.com/WeihanLi/SamplesInPractice/blob/master/NugetSample/NugetClientSdkSample.cs>
- <https://docs.microsoft.com/en-us/nuget/reference/nuget-client-sdk>
- <https://docs.microsoft.com/en-us/nuget/create-packages/set-package-type>
- <https://docs.microsoft.com/en-us/nuget/api/overview>
- <https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource>

