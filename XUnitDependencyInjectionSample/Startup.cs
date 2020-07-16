using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WeihanLi.Common;
using WeihanLi.Common.Services;

namespace XUnitDependencyInjectionSample
{
    public class Startup
    {
        // 配置 Host 注册，优先调用
        // 也可以修改方法的返回值为  `IHostBuilder`，返回一个 全新的 hostBuilder，
        // 有点类似于 asp.net core 2.x 里 Startup 里的 ConfigureServices 方法
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
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
