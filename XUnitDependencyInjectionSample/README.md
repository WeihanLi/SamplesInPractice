# 在 XUnit 中使用依赖注入

## Intro

今天介绍一个基于xunit和微软依赖注入框架的“真正”的依赖注入使用方式 ——— `Xunit.DependencyInjection`, 来自大师的作品，

可以让你在测试代码里使用依赖注入像 asp.net core 一样轻松

## 使用一览

### 包引用

在 xunit 测试项目里添加对 `Xunit.DependencyInjection` 的引用

``` console
dotnet add package Xunit.DependencyInjection
```

### 新建自己的 Startup

需要实现自己的一个 `Startup` ，在 `Startup` 里进行服务注册和初始化

``` csharp
namespace XUnitDependencyInjectionSample
{
    public class Startup
    {
        // 自定义 Host 构建，可以没有这样方法
        // 没有这个方法会使用一个默认的 hostBuilder
        public IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    // 注册配置
                    builder
                        .AddInMemoryCollection(new Dictionary<string, string>()
                        {
                            {"UserName", "Alice"}
                        })
                        .AddJsonFile("appsettings.json")
                        ;
                })
                .ConfigureServices((context, services) =>
                {
                    // 注册自定义服务
                    services.AddSingleton<IIdGenerator, GuidIdGenerator>();
                    if (context.Configuration.GetAppSetting<bool>("XxxEnabled"))
                    {
                        services.AddSingleton<IUserIdProvider, EnvironmentUserIdProvider>();
                    }
                })
                ;
        }

        // 支持的形式：
        // ConfigureServices(IServiceCollection services)
        // ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        // ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        {
            services.TryAddSingleton<CustomService>();
        }

        // 可以添加方法参数，会自动从注册的服务中获取服务实例
        public void Configure(IServiceProvider applicationServices, IIdGenerator idGenerator)
        {
            // 有一些测试数据要初始化可以放在这里
            // InitData();
        }
    }
}
```

`Startup` 和 asp.net core 里的 `Startup` 类似，

会多一个 `CreateHostBuilder` 的方法，允许用户自定义 Host 的构建，也可以没有这个方法

`ConfigureServices` 方法允许用户增加 `HostBuilderContext` 作为参数，可以通过 `hostBuilderContext` 来获取配置信息，也可以在 `CreateHostBuilder` 里注册也是一样的

注册配置/服务和 asp.net core 里一模一样，有数据或配置需要在项目启动时初始化的，可以放在 `Configure`  方法做，有点类似于 asp.net core 里 `Startup` 中的 `Configure` 方法，

只是这里我们不需要配置 asp.net core 的请求管道

## Startup 的寻找方法

默认的 `Startup` 通常是 `ProjectName.Startup`，通常在项目根目录下创建一个 `Startup` 是不需要配置的，如果不是或不起作用，可以参考下面 Startup 的寻找规则

如果要使用一个特别的 `Startup`, 你可以通过在项目文件的 `PropertyGroup` 部分定义 `XunitStartupAssembly` 和 `XunitStartupFullName`，具体规则如下

``` xml
<Project>
  <PropertyGroup>
    <XunitStartupAssembly>Abc</XunitStartupAssembly>
    <XunitStartupFullName>Xyz</XunitStartupFullName>
  </PropertyGroup>
</Project>
```

| XunitStartupAssembly | XunitStartupFullName | Startup |
| ------- | ------ | ------ |
|   |   | Your.Test.Project.Startup, Your.Test.Project |
| Abc |   | Abc.Startup, Abc |
|   | Xyz | Xyz, Your.Test.Project |
| Abc | Xyz | Xyz, Abc |

### 开始在测试代码里使用依赖注入吧

上面的 `Startup` 配置好以后就可以在测试代码里尽情使用依赖注入了，来看下面的两个示例：

首先我们可以测试一下内置的服务，就拿 `IConfiguration` 来测试吧

![IConfiguration service](https://img2020.cnblogs.com/blog/489462/202007/489462-20200702142827793-1584338140.png)

再来测试一下我们自定义注册的服务：

![custom services](https://img2020.cnblogs.com/blog/489462/202007/489462-20200702142850012-1457723391.png)

`IOutputHelper` 是 xunit 提供的，可以在执行测试的时候输出一段文本（使用 `Console.WriteLine` 是看不到输出的哦）

来看一下测试结果

![testoutput](https://img2020.cnblogs.com/blog/489462/202007/489462-20200702143023628-1876779215.png)

## 实现原理

`Xunit.DependencyInjection` 是一个开源项目，你可以在 Github 上获取到源码 <https://github.com/pengweiqhca/Xunit.DependencyInjection> 

`Xunit.DependencyInjection` 重写了一套基于 `Microsoft.Extensions.DependencyInjection` `TestFramework`，

使得测试执行可以支持依赖注入的方式，

在构建测试类时可以从注册的服务中获取构造器所需要的参数

在构建测试方法的时候也可以通过指定 `FromServices` 来从注册的服务中获取对应的服务从而实现方法参数的注入

从 5.0 版本开始直接依赖于 `Microsoft.Extensions.Hosting`，使用通用主机来构建依赖注入测试框架，

这样使得我们更方便集成 `Configuration` ，更像 asp.net core 的配置，更简洁

## More

大师写的项目真心不错，但是大师太低调了，写的很多很实用的项目，携程的阿波罗的 dotnetcore 支持就是大师一直在维护

大家快去 [Github](https://github.com/pengweiqhca) follow 他吧

## Reference

- <https://github.com/pengweiqhca/Xunit.DependencyInjection>
- <https://github.com/WeihanLi/SamplesInPractice/tree/master/XUnitDependencyInjectionSample>
