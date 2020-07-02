# 在 XUnit 中使用依赖注入

## Intro

之前写过一篇 xunit 的依赖注入相关的文章，但是实际使用起来不是那么方便

今天介绍一个基于xunit和微软依赖注入框架的“真正”的依赖注入使用方式 ——— `Xunit.DependencyInjection`, 来自大师的作品，让你在测试代码里使用依赖注入像 asp.net core 一样轻松

## 使用一览

### 包引用

在 xunit 测试项目里添加对 `Xunit.DependencyInjection` 的引用

```bash
dotnet add package Xunit.DependencyInjection
```

### 新建自己的 Startup

需要实现自己的一个 `Startup` ，在 `Startup` 里进行服务注册和初始化

``` csharp
// 这里需要指定一个 assembly attribute 用来让 xunit 寻找测试入口，
// TestFramework 第一个参数是 Startup 类的 FullName（包含命名空间），第二个参数是测试项目的程序集名称
[assembly: TestFramework("XUnitDependencyInjectionSample.Startup", "XUnitDependencyInjectionSample")]

namespace XUnitDependencyInjectionSample
{
    // Startup 需要继承于 DependencyInjectionTestFramework
    public class Startup : DependencyInjectionTestFramework
    {
        public Startup(IMessageSink messageSink) : base(messageSink)
        {
        }

        // 重写 CreateHostBuilder 这个方法，在这里进行配置注册和服务注册
        protected override IHostBuilder CreateHostBuilder(AssemblyName assemblyName)
        {
            var hostBuilder = base.CreateHostBuilder(assemblyName);
            hostBuilder
                // 注册配置
                .ConfigureAppConfiguration(builder =>
                {
                    builder
                        .AddInMemoryCollection(new Dictionary<string, string>()
                        {
                            {"UserName", "Alice"}
                        })
                        .AddJsonFile("appsettings.json")
                        ;
                })
                // 注册自定义服务
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IIdGenerator, GuidIdGenerator>();
                    if (context.Configuration.GetAppSetting<bool>("XxxEnabled"))
                    {
                        services.AddSingleton<IUserIdProvider, EnvironmentUserIdProvider>();
                    }
                })
                ;

            return hostBuilder;
        }

        protected override void Configure(IServiceProvider provider)
        {
            // 有一些测试数据需要初始化可以放在这里
        }
    }
}

```

`CreateHostBuilder` 其实就是 asp.net core 里的创建一个 `HostBuilder` ，注册配置/服务和 asp.net core 里一模一样，有数据或配置需要在项目启动时初始化的，可以放在 `Configure`  方法做，有点类似于 asp.net core 里 `Startup` 中的 `Configure` 方法，只是这里我们不需要配置 asp.net core 的请求管道

### 开始在测试代码里使用依赖注入吧

上面的 `Startup` 配置好以后就可以在测试代码里尽情使用依赖注入了，来看下面的两个示例：

首先我们可以测试一下内置的服务，就拿 `IConfiguration` 来测试吧

![](https://img2020.cnblogs.com/blog/489462/202007/489462-20200702142827793-1584338140.png)

再来测试一下我们自定义注册的服务：

![](https://img2020.cnblogs.com/blog/489462/202007/489462-20200702142850012-1457723391.png)

`IOutputHelper` 是 xunit 提供的，可以在执行测试的时候输出一段文本（使用 `Console.WriteLine` 是看不到输出的哦）

来看一下测试结果

![](https://img2020.cnblogs.com/blog/489462/202007/489462-20200702143023628-1876779215.png)


## 实现原理

`Xunit.DependencyInjection` 是一个开源项目，你可以在 Github 上获取到源码 <https://github.com/pengweiqhca/Xunit.DependencyInjection> 


`Xunit.DependencyInjection` 重写了一套基于 `Microsoft.Extensions.DependencyInjection` `TestFramework`，

使得测试执行可以支持依赖注入的方式，

在构建测试类时可以从注册的服务中获取构造器所需要的参数

在构建测试方法的时候也可以通过指定 `FromServices` 来从注册的服务中获取对应的服务从而实现方法参数的注入


重写的 xunit 的类如下

![](https://img2020.cnblogs.com/blog/489462/202007/489462-20200702143058938-1751489425.png)


从 5.0 版本开始直接依赖于 `Microsoft.Extensions.Hosting`，使用通用主机来构建依赖注入测试框架，

这样使得我们更方便集成 `Configuration` ，更像 asp.net core 的配置，更简洁

## More

大师写的项目真心不错，但是大师太低调了，写的很多很实用的项目，携程的阿波罗的 dotnetcore 支持就是大师一直在维护，，大家快去 [Github](https://github.com/pengweiqhca) follow 他吧


大师最近在写一个 `NetCache` 的开源项目，一个缓存框架，感兴趣的可以去看一下，给大师提点 feature ，

项目地址：<https://github.com/pengweiqhca/NetCache>


## Reference

- <https://github.com/pengweiqhca/Xunit.DependencyInjection>
- <https://github.com/WeihanLi/SamplesInPractice/tree/master/XUnitDependencyInjectionSample>
- <https://github.com/pengweiqhca/NetCache>
