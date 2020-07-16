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
        // 自定义 HostBuilder ，可以没有这个方法，没有这个方法会使用默认的 hostBuilder
        // public IHostBuilder CreateHostBuilder()
        // {
        //     return new HostBuilder()
        //         .ConfigureAppConfiguration(builder =>
        //         {
        //             // 注册配置
        //             builder
        //                 .AddInMemoryCollection(new Dictionary<string, string>()
        //                 {
        //                     {"UserName", "Alice"}
        //                 })
        //                 .AddJsonFile("appsettings.json")
        //                 ;
        //         })
        //         .ConfigureServices((context, services) =>
        //         {
        //             // 注册自定义服务
        //             services.AddSingleton<IIdGenerator, GuidIdGenerator>();
        //             if (context.Configuration.GetAppSetting<bool>("XxxEnabled"))
        //             {
        //                 services.AddSingleton<IUserIdProvider, EnvironmentUserIdProvider>();
        //             }
        //         })
        //         ;
        // }

        // 自定义 host 构建
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
