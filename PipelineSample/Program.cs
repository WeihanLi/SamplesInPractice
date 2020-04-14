using System;
using System.Linq;
using System.Threading.Tasks;

namespace PipelineSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PipelineBuilderTest();
            AsyncPipelineBuilderTest().Wait();

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static void PipelineBuilderTest()
        {
            var requestContext = new RequestContext()
            {
                RequesterName = "Kangkang",
                Hour = 12,
            };

            var builder = PipelineBuilder<RequestContext>.New(context =>
                    {
                        Console.WriteLine($"{context.RequesterName} {context.Hour}h apply failed");
                    })
                    .Use((context, next) =>
                    {
                        if (context.Hour < 2)
                        {
                            Console.WriteLine("pass 1");
                        }
                        else
                        {
                            next();
                        }
                    })
                    .Use((context, next) =>
                    {
                        if (context.Hour < 4)
                        {
                            Console.WriteLine("pass 2");
                        }
                        else
                        {
                            next();
                        }
                    })
                    .Use((context, next) =>
                    {
                        if (context.Hour < 6)
                        {
                            Console.WriteLine("pass 3");
                        }
                        else
                        {
                            next();
                        }
                    })
                ;
            var requestPipeline = builder.Build();
            foreach (var i in Enumerable.Range(1, 8))
            {
                Console.WriteLine($"--------- h:{i} apply Pipeline------------------");
                requestContext.Hour = i;
                requestPipeline.Invoke(requestContext);
                Console.WriteLine("----------------------------");
            }
        }

        private static async Task AsyncPipelineBuilderTest()
        {
            var requestContext = new RequestContext()
            {
                RequesterName = "Michael",
                Hour = 12,
            };

            var builder = AsyncPipelineBuilder<RequestContext>.New(context =>
                    {
                        Console.WriteLine($"{context.RequesterName} {context.Hour}h apply failed");
                        return Task.CompletedTask;
                    })
                    .Use(async (context, next) =>
                    {
                        if (context.Hour < 2)
                        {
                            Console.WriteLine("pass 1");
                        }
                        else
                        {
                            await next();
                        }
                    })
                    .Use(async (context, next) =>
                    {
                        if (context.Hour < 4)
                        {
                            Console.WriteLine("pass 2");
                        }
                        else
                        {
                            await next();
                        }
                    })
                    .Use(async (context, next) =>
                    {
                        if (context.Hour < 6)
                        {
                            Console.WriteLine("pass 3");
                        }
                        else
                        {
                            await next();
                        }
                    })
                ;
            var requestPipeline = builder.Build();
            foreach (var i in Enumerable.Range(1, 8))
            {
                Console.WriteLine($"--------- h:{i} apply AsyncPipeline------------------");
                requestContext.Hour = i;
                await requestPipeline.Invoke(requestContext);
                Console.WriteLine("----------------------------");
            }
        }
    }
}
