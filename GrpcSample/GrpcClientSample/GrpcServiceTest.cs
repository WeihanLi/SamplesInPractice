using System;
using System.Threading.Tasks;
using Greet.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Helpers;

namespace GrpcClientSample
{
    internal class GrpcServiceTest
    {
        public static async Task MainTest()
        {
            // http channel
            await InvokeHelper.TryInvokeAsync(HttpTest);

            // https
            await InvokeHelper.TryInvokeAsync(HttpsTest);

            // GrpcClientFactory
            await InvokeHelper.TryInvokeAsync(GrpcClientFactoryTest);
        }

        private static async Task HttpTest()
        {
            var httpChannel = GrpcChannel.ForAddress("http://localhost:5000");
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var client = new Greeter.GreeterClient(httpChannel);
            var reply = await client.SayHelloAsync(new HelloRequest()
            {
                Name = "Http test"
            });
            Console.WriteLine(reply.Message);
        }

        private static async Task GrpcClientFactoryTest()
        {
            var services = new ServiceCollection();

            services.AddGrpcClient<Greeter.GreeterClient>(o => o.Address = new Uri("https://localhost:5001"))
                .ConfigureChannel(o => { })
                ;
            await using var serviceProvider = services.BuildServiceProvider();

            var client = serviceProvider.GetRequiredService<Greeter.GreeterClient>();
            var reply = await client.SayHelloAsync(new HelloRequest()
            {
                Name = "GrpcClientFactory test"
            });
            Console.WriteLine(reply.Message);
        }

        private static async Task HttpsTest()
        {
            var httpsChannel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(httpsChannel);
            var reply = await client.SayHelloAsync(new HelloRequest()
            {
                Name = "Https test"
            });
            Console.WriteLine(reply.Message);
        }
    }
}
