using System;
using System.Threading;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;

namespace MiniAspNetCore
{
    public class Program
    {
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            Console.CancelKeyPress += OnExit;

            var host = WebHostBuilder.CreateDefault()
                .ConfigureServices((configuration, services) =>
                {
                })
                .ConfigureApplication((configuration, pipeline) =>
                {
                    pipeline.Use((context, next) => context.Response.WriteAsync("Hello Mini Asp.Net Core").AsTask());
                })
                .Build();
            await host.RunAsync(cts.Token);
        }

        private static void OnExit(object sender, EventArgs e)
        {
            cts.Cancel();
        }
    }
}
