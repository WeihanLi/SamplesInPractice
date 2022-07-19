using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace KubernetesClientSample
{
    public class PortForwardSample
    {
        private readonly IKubernetes _kubernetes;

        public PortForwardSample(IKubernetes kubernetes)
        {
            _kubernetes = kubernetes;
        }
        
        public async Task MainTest()
        {
            var pod = (await _kubernetes.CoreV1.ListNamespacedPodAsync("default")).Items
                .First(x => x.Name().Equals("reservation", StringComparison.Ordinal));

            using var cts = new CancellationTokenSource();
            var portForwardTask = PortForward(_kubernetes, pod, cts.Token, 8000);

            try
            {
                using var httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(10)};
                while (true)
                {
                    try
                    {
                        var response = await httpClient.GetAsync("http://localhost:8000/api/notice", cts.Token);
                        Console.WriteLine(response.StatusCode);
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(await response.Content.ReadAsStringAsync(cts.Token));
                            break;
                        }
                    }
                    catch
                    {
                        //
                    }

                    Console.WriteLine("Waiting for port-forward ready...");
                    await Task.Delay(1000, cts.Token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                cts.Cancel();
            }

            // wait for portForward exit
            try
            {
                await portForwardTask;
            }
            catch
            {
                //
            }
        }

        // Port-forward, modified from https://github.com/kubernetes-client/csharp/blob/master/examples/portforward/PortForward.cs#L24
        private static async Task PortForward(IKubernetes client, V1Pod pod, CancellationToken cancellationToken,
            int hostPort)
        {
            Console.WriteLine($"Port-forward started for pod {pod.Metadata.Name}");
            // Note this is single-threaded, it won't handle concurrent requests well...
            var webSocket = await client.WebSocketNamespacedPodPortForwardAsync(pod.Metadata.Name, pod.Namespace(),
                new[] {80}, "v4.channel.k8s.io", cancellationToken: cancellationToken);
            var demux = new StreamDemuxer(webSocket, StreamType.PortForward);
            demux.Start();

            var stream = demux.GetStream((byte?) 0, (byte?) 0);

            var ipAddress = IPAddress.Loopback;
            var localEndPoint = new IPEndPoint(ipAddress, hostPort);
            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(100);

            var handler = listener.Accept();

            cancellationToken.Register(() =>
            {
                try
                {
                    handler.Close();
                    listener.Close();
                    handler.Dispose();
                    listener.Dispose();

                    demux.Dispose();
                    webSocket.Dispose();
                }
                catch
                {
                    //
                }

                Console.WriteLine("Port-forward closed");
            });

            cancellationToken.ThrowIfCancellationRequested();

            // Note this will only accept a single connection
            var accept = Task.Run(() =>
            {
                var bytes = new byte[4096];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var bytesRec = handler.Receive(bytes);
                    stream.Write(bytes, 0, bytesRec);
                    if (bytesRec == 0 || Encoding.ASCII.GetString(bytes, 0, bytesRec)
                        .IndexOf("<EOF>", StringComparison.OrdinalIgnoreCase) > -1) break;
                }
            }, cancellationToken);
            var copy = Task.Run(() =>
            {
                var buff = new byte[4096];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var read = stream.Read(buff, 0, 4096);
                    handler.Send(buff, read, 0);
                }
            }, cancellationToken);

            await Task.WhenAny(accept, copy);
        }
    }
}
