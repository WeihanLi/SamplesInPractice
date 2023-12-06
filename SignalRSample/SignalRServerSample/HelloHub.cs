using Microsoft.AspNetCore.SignalR;
using SignalRSample;

namespace SignalRServerSample;

public sealed class HelloHub : Hub<IHelloClient>, IHelloServer
{
    public async Task Hello(HelloModel model)
    {
        await Clients.Client(Context.ConnectionId)
            .Hello(new HelloModel()
            {
                Name = "server",
                Message = $"{model.Name}: {model.Message}"
            });
    }
}
