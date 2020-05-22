# 动手写一个简版 asp.net core

## Intro

之前看到过蒋金楠老师的一篇 200 行代码带你了解 asp.net core 框架，最近参考蒋老师和 Edison 的文章和代码，结合自己对 asp.net core 的理解 ，最近自己写了一个 MiniAspNetCore ，写篇文章总结一下。

## HttpContext

`HttpContext` 可能是最为常用的一个类了，`HttpContext` 是请求上下文，包含了所有的请求信息以及响应信息，以及一些自定义的用于在不同中间件中传输数据的信息

来看一下 `HttpContext` 的定义:

``` csharp
public class HttpContext
{
    public IServiceProvider RequestServices { get; set; }

    public HttpRequest Request { get; set; }

    public HttpResponse Response { get; set; }

    public IFeatureCollection Features { get; set; }

    public HttpContext(IFeatureCollection featureCollection)
    {
        Features = featureCollection;
        Request = new HttpRequest(featureCollection);
        Response = new HttpResponse(featureCollection);
    }
}
```

`HttpRequest` 即为请求信息对象，包含了所有请求相关的信息，

`HttpResponse` 为响应信息对象，包含了请求对应的响应信息

`RequestServices` 为 asp.net core 里的`RequestServices`，代表当前请求的服务提供者，可以使用它来获取具体的服务实例

`Features` 为 asp.net core 里引入的对象，可以用来在不同中间件中传递信息和用来解耦合

，下面我们就来看下 `HttpRequest` 和 `HttpResponse` 是怎么实现的

HttpRequest:

``` csharp
public class HttpRequest
{
    private readonly IRequestFeature _requestFeature;

    public HttpRequest(IFeatureCollection featureCollection)
    {
        _requestFeature = featureCollection.Get<IRequestFeature>();
    }

    public Uri Url => _requestFeature.Url;

    public NameValueCollection Headers => _requestFeature.Headers;

    public string Method => _requestFeature.Method;

    public string Host => _requestFeature.Url.Host;

    public Stream Body => _requestFeature.Body;
}
```

HttpResponse:

``` csharp
public class HttpResponse
{
    private readonly IResponseFeature _responseFeature;

    public HttpResponse(IFeatureCollection featureCollection)
    {
        _responseFeature = featureCollection.Get<IResponseFeature>();
    }

    public bool ResponseStarted => _responseFeature.Body.Length > 0;

    public int StatusCode
    {
        get => _responseFeature.StatusCode;
        set => _responseFeature.StatusCode = value;
    }

    public async Task WriteAsync(byte[] responseBytes)
    {
        if (_responseFeature.StatusCode <= 0)
        {
            _responseFeature.StatusCode = 200;
        }
        if (responseBytes != null && responseBytes.Length > 0)
        {
            await _responseFeature.Body.WriteAsync(responseBytes);
        }
    }
}
```

## Features

上面我们提供我们可以使用 `Features` 在不同中间件中传递信息和解耦合

由上面 `HttpRequest`/`HttpResponse` 的代码我们可以看出来，`HttpRequest` 和 `HttpResponse` 其实就是在 `IRequestFeature` 和 `IResponseFeature` 的基础上封装了一层，真正的核心其实是 `IRequestFeature`/`IResponseFeature` ，而这里使用接口就很好的实现了解耦，可以根据不同的 WebServer 使用不同的 `RequestFeature`/`ResponseFeature`，来看下 `IRequestFeature`/`IResponseFeature` 的实现

``` csharp
public interface IRequestFeature
{
    Uri Url { get; }

    string Method { get; }

    NameValueCollection Headers { get; }

    Stream Body { get; }
}

public interface IResponseFeature
{
    public int StatusCode { get; set; }

    NameValueCollection Headers { get; set; }

    public Stream Body { get; }
}
```

> 这里的实现和 asp.net core 的实际的实现方式应该不同，asp.net core 里 Headers 同一个 Header 允许有多个值，asp.net core 里是 StringValues 来实现的，这里简单处理了，使用了一个 `NameValueCollection` 对象

上面提到的 `Features` 是一个 `IFeatureCollection` 对象，相当于是一系列的 `Feature` 对象组成的，来看下  `FeatureCollection` 的定义：

``` csharp
public interface IFeatureCollection : IDictionary<Type, object> { }

public class FeatureCollection : Dictionary<Type, object>, IFeatureCollection
{
}
```

这里 `IFeatureCollection` 直接实现 `IDictionary<Type, object>` ，通过一个字典 Feature 类型为 Key，Feature 对象为 Value 的字典来保存

为了方便使用，可以定义两个扩展方法来方便的Get/Set

``` csharp
public static class FeatureExtensions
{
    public static IFeatureCollection Set<TFeature>(this IFeatureCollection featureCollection, TFeature feature)
    {
        featureCollection[typeof(TFeature)] = feature;
        return featureCollection;
    }

    public static TFeature Get<TFeature>(this IFeatureCollection featureCollection)
    {
        var featureType = typeof(TFeature);
        return featureCollection.ContainsKey(featureType) ? (TFeature)featureCollection[featureType] : default(TFeature);
    }
}
```

## Web服务器

![](https://img2018.cnblogs.com/blog/19327/201901/19327-20190128080856626-710206291.jpg)

上面我们已经提到了 Web 服务器通过 `IRequestFeature`/`IResponseFeature` 来实现不同 web 服务器和应用程序的解耦，web 服务器只需要提供自己的 `RequestFeature`/`ResponseFeature` 即可

为了抽象不同的 Web 服务器，我们需要定义一个 `IServer` 的抽象接口，定义如下：

``` csharp
public interface IServer
{
    Task StartAsync(Func<HttpContext, Task> requestHandler, CancellationToken cancellationToken = default);
}
```

`IServer` 定义了一个 `StartAsync` 方法，用来启动 Web服务器，

`StartAsync` 方法有两个参数，一个是 requestHandler，是一个用来处理请求的委托，另一个是取消令牌用来停止 web 服务器

示例使用了 `HttpListener` 来实现了一个简单 Web 服务器，`HttpListenerServer` 定义如下：

``` csharp
public class HttpListenerServer : IServer
{
    private readonly HttpListener _listener;
    private readonly IServiceProvider _serviceProvider;

    public HttpListenerServer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _listener = new HttpListener();
        var urls = configuration.GetAppSetting("ASPNETCORE_URLS")?.Split(';');
        if (urls != null && urls.Length > 0)
        {
            foreach (var url in urls
                     .Where(u => u.IsNotNullOrEmpty())
                     .Select(u => u.Trim())
                     .Distinct()
                    )
            {
                // Prefixes must end in a forward slash ("/")
                // https://stackoverflow.com/questions/26157475/use-of-httplistener
                _listener.Prefixes.Add(url.EndsWith("/") ? url : $"{url}/");
            }
        }
        else
        {
            _listener.Prefixes.Add("http://localhost:5100/");
        }

        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(Func<HttpContext, Task> requestHandler, CancellationToken cancellationToken = default)
    {
        _listener.Start();
        if (_listener.IsListening)
        {
            Console.WriteLine("the server is listening on ");
            Console.WriteLine(_listener.Prefixes.StringJoin(","));
        }
        while (!cancellationToken.IsCancellationRequested)
        {
            var listenerContext = await _listener.GetContextAsync();

            var featureCollection = new FeatureCollection();
            featureCollection.Set(listenerContext.GetRequestFeature());
            featureCollection.Set(listenerContext.GetResponseFeature());

            using (var scope = _serviceProvider.CreateScope())
            {
                var httpContext = new HttpContext(featureCollection)
                {
                    RequestServices = scope.ServiceProvider,
                };

                await requestHandler(httpContext);
            }
            listenerContext.Response.Close();
        }
        _listener.Stop();
    }
}
```

`HttpListenerServer` 实现的 `RequestFeature`/`ResponseFeatue`

``` csharp
public class HttpListenerRequestFeature : IRequestFeature
{
    private readonly HttpListenerRequest _request;

    public HttpListenerRequestFeature(HttpListenerContext listenerContext)
    {
        _request = listenerContext.Request;
    }

    public Uri Url => _request.Url;
    public string Method => _request.HttpMethod;
    public NameValueCollection Headers => _request.Headers;
    public Stream Body => _request.InputStream;
}

public class HttpListenerResponseFeature : IResponseFeature
{
    private readonly HttpListenerResponse _response;

    public HttpListenerResponseFeature(HttpListenerContext httpListenerContext)
    {
        _response = httpListenerContext.Response;
    }

    public int StatusCode { get => _response.StatusCode; set => _response.StatusCode = value; }

    public NameValueCollection Headers
    {
        get => _response.Headers;
        set
        {
            _response.Headers = new WebHeaderCollection();
            foreach (var key in value.AllKeys)
                _response.Headers.Add(key, value[key]);
        }
    }

    public Stream Body => _response.OutputStream;
}

```

为了方便使用，为 `HttpListenerContext` 定义了两个扩展方法，就是上面 `HttpListenerServer` 中的 `GetRequestFeature`/`GetResponseFeature`：

``` csharp
public static class HttpListenerContextExtensions
{
    public static IRequestFeature GetRequestFeature(this HttpListenerContext context)
    {
        return new HttpListenerRequestFeature(context);
    }

    public static IResponseFeature GetResponseFeature(this HttpListenerContext context)
    {
        return new HttpListenerResponseFeature(context);
    }
}
```

## RequestDelegate

在上面的 `IServer` 定义里有一个 requestHandler 的 对象，在 asp.net core 里是一个名称为 `RequestDelegate` 的对象，而用来构建这个委托的在 asp.net core 里是 `IApplicationBuilder`，这些在蒋老师和 Edison 的文章和代码里都可以看到，这里我们只是简单介绍下，我在 MiniAspNetCore 的示例中没有使用这些对象，而是使用了自己抽象的 `PipelineBuilder` 和原始委托实现的

asp.net core 里 `RequestDelegate` 定义：

``` csharp
public delegate Task RequestDelegate(HttpContext context);
```

其实和我们上面定义用的 `Func<HttpContext, Task>` 是等价的

`IApplicationBuilder` 定义：

``` csharp
/// <summary>
/// Defines a class that provides the mechanisms to configure an application's request pipeline.
/// </summary>
public interface IApplicationBuilder
{
    /// <summary>
    /// Gets or sets the <see cref="T:System.IServiceProvider" /> that provides access to the application's service container.
    /// </summary>
    IServiceProvider ApplicationServices { get; set; }

    /// <summary>
    /// Gets the set of HTTP features the application's server provides.
    /// </summary>
    IFeatureCollection ServerFeatures { get; }

    /// <summary>
    /// Gets a key/value collection that can be used to share data between middleware.
    /// </summary>
    IDictionary<string, object> Properties { get; }

    /// <summary>
    /// Adds a middleware delegate to the application's request pipeline.
    /// </summary>
    /// <param name="middleware">The middleware delegate.</param>
    /// <returns>The <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.</returns>
    IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware);

    /// <summary>
    /// Creates a new <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" /> that shares the <see cref="P:Microsoft.AspNetCore.Builder.IApplicationBuilder.Properties" /> of this
    /// <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.
    /// </summary>
    /// <returns>The new <see cref="T:Microsoft.AspNetCore.Builder.IApplicationBuilder" />.</returns>
    IApplicationBuilder New();

    /// <summary>
    /// Builds the delegate used by this application to process HTTP requests.
    /// </summary>
    /// <returns>The request handling delegate.</returns>
    RequestDelegate Build();
}
```

我们这里没有定义 `IApplicationBuilder`，使用了简化抽象的 `IAsyncPipelineBuilder`，定义如下：

``` csharp
public interface IAsyncPipelineBuilder<TContext>
{
    IAsyncPipelineBuilder<TContext> Use(Func<Func<TContext, Task>, Func<TContext, Task>> middleware);

    Func<TContext, Task> Build();

    IAsyncPipelineBuilder<TContext> New();
}
```

对于 asp.net core 的中间件来说 ，上面的 `TContext` 就是 `HttpContext`，替换之后也就是下面这样的：

``` csharp
public interface IAsyncPipelineBuilder<HttpContext>
{
    IAsyncPipelineBuilder<HttpContext> Use(Func<Func<HttpContext, Task>, Func<HttpContext, Task>> middleware);

    Func<HttpContext, Task> Build();

    IAsyncPipelineBuilder<HttpContext> New();
}
```

是不是和 `IApplicationBuilder` 很像，如果不像可以进一步把 `Func<HttpContext, Task>` 使用 `RequestDelegate` 替换

``` csharp
public interface IAsyncPipelineBuilder<HttpContext>
{
    IAsyncPipelineBuilder<HttpContext> Use(Func<RequestDelegate, RequestDelegate> middleware);

    RequestDelegate Build();

    IAsyncPipelineBuilder<HttpContext> New();
}
```

最后再将接口名称替换一下：

``` csharp
public interface IApplicationBuilder1
{
    IApplicationBuilder1 Use(Func<RequestDelegate, RequestDelegate> middleware);

    RequestDelegate Build();

    IApplicationBuilder1 New();
}
```

至此，就完全可以看出来了，这 `IAsyncPipelineBuilder<HttpContext>` 就是一个简版的 `IApplicationBuilder`

`IAsyncPipelineBuilder` 和 `IApplicationBuilder` 的作用是将注册的多个中间件构建成一个请求处理的委托

![](https://img2020.cnblogs.com/blog/489462/202005/489462-20200522120639290-1183675935.png)

中间件处理流程：

![](https://img2020.cnblogs.com/blog/489462/202005/489462-20200522120936921-489107143.png)


更多关于 PipelineBuilder 构建中间件的信息可以查看 [让 .NET 轻松构建中间件模式代码](https://www.cnblogs.com/weihanli/p/12700006.html) 了解更多

## WebHost

通过除了 Web 服务器之外，还有一个 Web Host 的概念，可以简单的这样理解，一个 Web 服务器上可以有多个 Web Host，就像 IIS/nginx (Web Server) 可以 host 多个站点

可以说 WebHost 离我们的应用更近，所以我们还需要 `IHost` 来托管应用

``` csharp
public interface IHost
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
```

`WebHost` 定义：

``` csharp
public class WebHost : IHost
{
    private readonly Func<HttpContext, Task> _requestDelegate;
    private readonly IServer _server;

    public WebHost(IServiceProvider serviceProvider, Func<HttpContext, Task> requestDelegate)
    {
        _requestDelegate = requestDelegate;
        _server = serviceProvider.GetRequiredService<IServer>();
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await _server.StartAsync(_requestDelegate, cancellationToken).ConfigureAwait(false);
    }
}
```

为了方便的构建 `Host`对象，引入了 `HostBuilder` 来方便的构建一个 `Host`，定义如下：

``` csharp
public interface IHostBuilder
{
    IHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction);

    IHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureAction);

    IHostBuilder Initialize(Action<IConfiguration, IServiceProvider> initAction);

    IHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configureAction);

    IHost Build();
}

```

`WebHostBuilder`：

``` csharp
public class WebHostBuilder : IHostBuilder
{
    private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    private Action<IConfiguration, IServiceProvider> _initAction = null;

    private readonly IAsyncPipelineBuilder<HttpContext> _requestPipeline = PipelineBuilder.CreateAsync<HttpContext>(context =>
    {
        context.Response.StatusCode = 404;
        return Task.CompletedTask;
    });

    public IHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction)
    {
        configAction?.Invoke(_configurationBuilder);
        return this;
    }

    public IHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureAction)
    {
        if (null != configureAction)
        {
            var configuration = _configurationBuilder.Build();
            configureAction.Invoke(configuration, _serviceCollection);
        }

        return this;
    }

    public IHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configureAction)
    {
        if (null != configureAction)
        {
            var configuration = _configurationBuilder.Build();
            configureAction.Invoke(configuration, _requestPipeline);
        }
        return this;
    }

    public IHostBuilder Initialize(Action<IConfiguration, IServiceProvider> initAction)
    {
        if (null != initAction)
        {
            _initAction = initAction;
        }

        return this;
    }

    public IHost Build()
    {
        var configuration = _configurationBuilder.Build();
        _serviceCollection.AddSingleton<IConfiguration>(configuration);
        var serviceProvider = _serviceCollection.BuildServiceProvider();

        _initAction?.Invoke(configuration, serviceProvider);

        return new WebHost(serviceProvider, _requestPipeline.Build());
    }

    public static WebHostBuilder CreateDefault(string[] args)
    {
        var webHostBuilder = new WebHostBuilder();
        webHostBuilder
            .ConfigureConfiguration(builder => builder.AddJsonFile("appsettings.json", true, true))
            .UseHttpListenerServer()
            ;

        return webHostBuilder;
    }
}
```



> 这里的示例我在 `IHostBuilder` 里增加了一个 `Initialize` 的方法来做一些初始化的操作，我觉得有些数据初始化配置初始化等操作应该在这里操作，而不应该在 `Startup` 的 `Configure` 方法里处理，这样 `Configure` 方法可以更纯粹一些，只配置 asp.net core 的请求管道，这纯属个人意见，没有对错之分
>
> 这里 Host 的实现和 asp.net core 的实现不同，有需要的可以深究源码，在 asp.net core 2.x 的版本里是有一个 `IWebHost` 的，在 asp.net core 3.x 以及 .net 5 里是没有 `IWebHost` 的取而代之的是通用主机 `IHost`， 通过实现了一个 `IHostedService` 来实现 `WebHost` 的

## Run

运行示例代码：

``` csharp
public class Program
{
    private static readonly CancellationTokenSource Cts = new CancellationTokenSource();

    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += OnExit;

        var host = WebHostBuilder.CreateDefault(args)
            .ConfigureServices((configuration, services) =>
            {
            })
            .ConfigureApplication((configuration, app) =>
            {
                app.When(context => context.Request.Url.PathAndQuery.StartsWith("/favicon.ico"), pipeline => { });

                app.When(context => context.Request.Url.PathAndQuery.Contains("test"),
                    p => { p.Run(context => context.Response.WriteAsync("test")); });
                app
                    .Use(async (context, next) =>
                    {
                        await context.Response.WriteLineAsync($"middleware1, requestPath:{context.Request.Url.AbsolutePath}");
                        await next();
                    })
                    .Use(async (context, next) =>
                    {
                        await context.Response.WriteLineAsync($"middleware2, requestPath:{context.Request.Url.AbsolutePath}");
                        await next();
                    })
                    .Use(async (context, next) =>
                    {
                        await context.Response.WriteLineAsync($"middleware3, requestPath:{context.Request.Url.AbsolutePath}");
                        await next();
                    })
                    ;
                app.Run(context => context.Response.WriteAsync("Hello Mini Asp.Net Core"));
            })
            .Initialize((configuration, services) =>
            {
            })
            .Build();
        await host.RunAsync(Cts.Token);
    }

    private static void OnExit(object sender, EventArgs e)
    {
        Console.WriteLine("exiting ...");
        Cts.Cancel();
    }
}
```

在示例项目目录下执行 `dotnet run`，并访问 `http://localhost:5100/`:

![](https://img2020.cnblogs.com/blog/489462/202005/489462-20200522121759714-1340223664.png)

仔细观察浏览器 `console` 或 `network` 的话，会发现还有一个请求，浏览器会默认请求 `/favicon.ico` 获取网站的图标

![](https://img2020.cnblogs.com/blog/489462/202005/489462-20200522121925619-761432869.png)

因为我们针对这个请求没有任何中间件的处理，所以直接返回了 404

在访问 `/test`，可以看到和刚才的输出完全不同，因为这个请求走了另外一个分支，相当于 asp.net core 里 `Map`/`MapWhen` 的效果，另外 `Run` 代表里中间件的中断，不会执行后续的中间件

![](https://img2020.cnblogs.com/blog/489462/202005/489462-20200522122208721-151942406.png)


## More

上面的实现只是我在尝试写一个简版的 asp.net core 框架时的实现，和 asp.net core 的实现并不完全一样，如果需要请参考源码，上面的实现仅供参考，上面实现的源码可以在 Github 上获取 <https://github.com/WeihanLi/SamplesInPractice/tree/master/MiniAspNetCore>

asp.net core 源码：<https://github.com/dotnet/aspnetcore>

## Reference

- <https://www.cnblogs.com/artech/p/inside-asp-net-core-framework.html>
- <https://www.cnblogs.com/artech/p/mini-asp-net-core-3x.html>
- <https://www.cnblogs.com/edisonchou/p/aspnet_core_mini_implemention_introduction.html>
- <https://www.cnblogs.com/weihanli/p/12700006.html>
- <https://www.cnblogs.com/weihanli/p/12709603.html>
- <https://github.com/WeihanLi/SamplesInPractice/tree/master/MiniAspNetCore>
