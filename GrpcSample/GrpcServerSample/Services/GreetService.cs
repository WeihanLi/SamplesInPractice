using System.Threading.Tasks;
using Greet.V1;
using Grpc.Core;

namespace GrpcServerSample.Services
{
    public class GreetService : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply()
            {
                Message = $"Hello from {request.Name}"
            });
        }
    }
}
