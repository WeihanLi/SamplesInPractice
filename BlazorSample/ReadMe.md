# Blazor 从入门到放弃

## Intro

Blazor 是微软在 .NET 里推出的一个 WEB 客户端 UI 交互的框架，

使用 Blazor 你可以代替 JavaScript 来实现自己的页面交互逻辑，可以很大程度上进行 C# 代码的复用，Blazor 对于 .NET 开发人员来说是一个不错的选择。

## 托管模型

Blazor 有两种托管模式，一种是 Blazor Server 模式，基于 asp.net core 部署，客户端和服务器的交互通过 SignalR 来完成，来实现客户端 UI 的更新和行为的交互。

![Blazor Server runs .NET code on the server and interacts with the Document Object Model on the client over a SignalR connection](https://docs.microsoft.com/en-us/aspnet/core/blazor/index/_static/blazor-server.png?view=aspnetcore-5.0)

![Blazor Server](https://docs.microsoft.com/zh-cn/aspnet/core/blazor/hosting-models/_static/blazor-server.png?view=aspnetcore-5.0)

另外一种是 Blazor WebAssembly 模式， 将 Blazor 应用、其依赖项以及 .NET 运行时下载到浏览器， 应用将在浏览器线程中直接执行。

![Blazor WebAssembly runs .NET code in the browser with WebAssembly.](https://docs.microsoft.com/en-us/aspnet/core/blazor/index/_static/blazor-webassembly.png?view=aspnetcore-5.0)

![Blazor WebAssembly](https://docs.microsoft.com/zh-cn/aspnet/core/blazor/hosting-models/_static/blazor-webassembly.png?view=aspnetcore-5.0)

两种模式各有优缺点，但是个人觉得 WebAssembly 模式的 Blazor 意义更大一些

Blazor Server 托管模型具有以下优点：

- 下载项大小明显小于 Blazor WebAssembly 应用，且应用加载速度快得多。
- 应用可充分利用服务器功能，包括使用任何与 .NET Core 兼容的 API。
- 服务器上的 .NET Core 用于运行应用，因此调试等现有 .NET 工具可按预期正常工作。
- 支持瘦客户端。 例如，Blazor Server 应用适用于不支持 WebAssembly 的浏览器以及资源受限的设备。
- 应用的 .NET/C# 代码库（其中包括应用的组件代码）不适用于客户端。

Blazor Server 托管模型具有以下局限性：

- 通常延迟较高。 每次用户交互都涉及到网络跃点。
- 不支持脱机工作。 如果客户端连接失败，应用会停止工作。
- 如果具有多名用户，则应用扩缩性存在挑战。 服务器必须管理多个客户端连接并处理客户端状态(SignalR)。
- 需要 ASP.NET Core 服务器为应用提供服务。 无服务器部署方案不可行，例如通过内容分发网络 (CDN) 为应用提供服务的方案。

Blazor WebAssembly 托管模型具有以下优点：

- 没有 .NET 服务器端依赖，应用下载到客户端后即可正常运行。
- 可充分利用客户端资源和功能。
- 工作可从服务器转移到客户端。
- 无需 ASP.NET Core Web 服务器即可托管应用。 无服务器部署方案可行，例如通过内容分发网络 (CDN) 为应用提供服务的方案。

Blazor WebAssembly 托管模型具有以下局限性：

- 应用仅可使用浏览器功能。
- 需要可用的客户端硬件和软件（例如 WebAssembly 支持）。
- 下载项大小较大，应用加载耗时较长。
- .NET 运行时和工具支持不够完善。 例如，[.NET Standard](https://docs.microsoft.com/zh-cn/dotnet/standard/net-standard) 支持和调试方面存在限制。

## 项目结构

Blazor 结合了 Razor Page 的开发模式，可以使用 Razor 的语法，文件结构也和 Razor Page 的模式有些类似

Blazor 是以组件为核心的，页面所有的部分都是一个组件

Blazor WebAssembly 对应的 SDK 是 `Microsoft.NET.Sdk.BlazorWebAssembly`，来看一下具体的项目文件：

``` xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="6.0.0-preview.4.21253.5" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="6.0.0-preview.4.21253.5" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

![project-structure](C:\Users\Weiha\AppData\Roaming\Typora\typora-user-images\image-20210605140232943.png)

- `App.razor`: Blazor WebAssembly 应用根组件
- `Program.cs`: 配置应用 WebAssembly host 的入口文件
- `_Imports.razor`: 和 Razor Page 一样，可以在这里定义一些 Razor Page 或者组件里公用的 namespace

- `Pages`：包含可以路由到的页面，page 需要使用 `@page` 指令指定
- `Shared`：包含一些公共的组件或者样式定义
- `wwwroot`: 应用公共静态文件的根目录

## Routing

在页面组件上通过 `@page` 指令指定页面路由 `@page "/path"`，就会生成一个 `RouteAttribute` 以支持路由，路由支持像 asp.net core 一样的路由约束和 Path 参数

 ``` razor
 @page "/RouteParameter/{text}"
 
 <h1>Blazor is @Text!</h1>
 
 @code {
     [Parameter]
     public string Text { get; set; }
 }
 ```

``` razor
@page "/RouteParameter/{text?}"

<h1>Blazor is @Text!</h1>

@code {
    [Parameter]
    public string Text { get; set; }

    protected override void OnInitialized()
    {
        Text = Text ?? "fantastic";
    }
}
```

``` razor
@page "/user/{Id:int}"

<h1>User Id: @Id</h1>

@code {
    [Parameter]
    public int Id { get; set; }
}
```

### Catch-all

``` razor
@page "/catch-all/{*pageRoute}"

@code {
    [Parameter]
    public string PageRoute { get; set; }
}
```

## Interop

### Model Binding

最基本的我们需要了解如何做数据绑定，

```
<div>
  <input type="checkbox" checked="@item.IsCompleted" />
  <h4>@item.TodoTitle</h4> -- <span class="small">@item.CreatedTime.ToStandardTimeString()</span>
</div>
<div class="todo-item-details">
  <p>@item.TodoContent</p>
</div>
@code
{
    public List<TodoItem> TodoItems { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        TodoItems = await scheduler.GetAllTasks();
        await base.OnInitializedAsync();
    }
}
```

### Event Binding

在现在的 Blazor 里，事件绑定是偏向于使用原生的事件名，比如按钮的事件通过 `@onclick` 方式来绑定事件，例如下面的示例：

``` c#
<button @onclick="AddNewTodo" class="btn btn-info">Add new todo</button>
```

为 button 指定了一个 `onclick` 事件处理器

### Call JS method

执行 JS 方法有时候是不可缺少的一部分，因为很多组件都是 JS 的，借助于此，我们就可以直接调用  JS 的方法来实现一些组件功能，示例如下，分是否有返回值可以分为两类：

#### With return value

```
@inject IJSRuntime JS
@code {
    private MarkupString text;

    private async Task ConvertArray()
    {
        text = new(await JS.InvokeAsync<string>("convertArray", quoteArray));
    }
}
```

#### Without return value

```
@inject IJSRuntime JS
@code {
    private Random r = new();
    private string stockSymbol;
    private decimal price;

    private async Task SetStock()
    {
        stockSymbol = 
            $"{(char)('A' + r.Next(0, 26))}{(char)('A' + r.Next(0, 26))}";
        price = r.Next(1, 101);
        await JS.InvokeVoidAsync("displayTickerAlert1", stockSymbol, price);
    }
}
```

## More

更多关于 Blazor 相关的知识可以参考微软的文档

## References

- <https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-5.0>
- <https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-5.0>
- <https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-5.0>
- <https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-5.0>
- <https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-5.0>
- <https://docs.microsoft.com/en-us/dotnet/api/microsoft.jsinterop.ijsruntime?view=dotnet-plat-ext-6.0>

