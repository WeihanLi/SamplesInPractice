using System;
using System.Threading;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;

namespace MiniAspNetCore
{
    public class Program
    {
        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
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
                        .Use((context, next) =>
                        {
                            Console.WriteLine($"middleware1, requestPath:{context.Request.Url.PathAndQuery}");
                            return next();
                        })
                        .Use((context, next) =>
                        {
                            Console.WriteLine($"middleware2, requestPath:{context.Request.Url.PathAndQuery}");
                            return next();
                        })
                        .Use((context, next) =>
                        {
                            Console.WriteLine($"middleware3, requestPath:{context.Request.Url.PathAndQuery}");
                            return next();
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
            Cts.Cancel();
        }
    }
}
