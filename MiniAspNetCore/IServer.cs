using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniAspNetCore
{
    public interface IServer
    {
        Task StartAsync(Func<HttpContext, Task> requestHandler, CancellationToken cancellationToken = default);

        Task StopAsync();
    }
}
