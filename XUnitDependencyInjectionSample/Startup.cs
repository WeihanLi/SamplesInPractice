using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeihanLi.Common;
using WeihanLi.Common.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

[assembly: TestFramework("XUnitDependencyInjectionSample.Startup", "XUnitDependencyInjectionSample")]

namespace XUnitDependencyInjectionSample
{
    public class Startup : DependencyInjectionTestFramework
    {
        public Startup(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override IHostBuilder CreateHostBuilder(AssemblyName assemblyName)
        {
            var hostBuilder = base.CreateHostBuilder(assemblyName);
            hostBuilder
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
            // 有一些测试数据要初始化可以放在这里
        }
    }
}
