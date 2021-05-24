# 使用 KubernetesClient 操作 kubernetes

## Intro

我们的应用都是部署在 Kubernetes 上的，我们有一个服务内部有一层 `MemoryCache`，之前会依赖 Redis 的 Pub/Sub 来做缓存的更新，而 Redis 的 Pub/Sub 是一种不可靠的更新机制，容易发生消息丢失从而导致数据不一致的情况。

之前我们有一次问题就是因为这个导致的，在 k8s 集群里有几个 Pod 的数据还是老数据，导致接口拿到的数据有时候是正确的有时候是错误的，这就很尴尬了，发现有问题，每次都去一个一个Pod 去检查就很烦，于是就想写一个脚本或者小工具来自动检查所有集群所有 Pod 的返回值，于是就有了这篇文章的探索。

## 实现原理

Kubernetes 集群通过 API Server 对外提供了 REST API 以方便通过 API 来操作 Kubernetes 集群

![Components of Kubernetes](https://d33wubrfki0l68.cloudfront.net/2475489eaf20163ec0f54ddc1d92aa8d4c87c96b/e7c81/images/docs/components-of-kubernetes.svg)

![A diagram showing how the parts of a Kubernetes cluster relate to one another](https://www.redhat.com/cms/managed-files/kubernetes_diagram-v3-770x717_0.svg)

kubectl 实际工作方式就是一个和 API Server 进行交互的命令行工具，所以我们完全可以自己根据 Kubernetes 提供的 API 来实现我们需要的功能，而 Kubernetes 官方也维护了一个 dotnet 的客户端  `KubernetesClient`，从而我们可以少写很多代码，直接使用这个 SDK 就可以比较方便的对 Kubernetes 进行操作了。

想一下，如果我们使用 `kubectl` 的话要如何检查一个集群所有的 pod 的返回结果呢？

首先我们可以通过 `kubectl get pod` 来获取一个 pod 列表，拿到 pod 列表之后就可以依次访问各个 pod 的 API 拿返回结果了，这里我想到的有两种方式，一种是在 pod 里执行 `curl` 命令，访问 API 拿到返回的数据，另一种方式是针对 pod 进行 port-forward，然后访问 `localhost` 就可以请求接口拿到返回数据了。

最后选择的是 port-forward 的方式，因为有的容器里可能并没有 `curl`，不够通用，所以放弃了在容器里 `curl` 的方式。

## InspectSample

首先我们来看一下 `KubernetesClient` 基本的使用吧，来看一个简单的示例，遍历所有的 `namespace`，依次获取每个 `namespace` 下的 `pod`

首先我们需要构建一个 `IKubernetes` 实例以和 Kubenetes 进行通信，在此之前我们需要先构建 `KubernetesClientConfiguration`，基本使用如下：

``` c#
// 使用默认的配置，默认的 kubernetes 配置文件，Windows 是 `%PROFILE%/.kube/config`，Linux 是 `%HOME%/.kube/config`
var config = KubernetesClientConfiguration.BuildDefaultConfig();
// 使用指定的配置文件
//var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(file);
IKubernetes kubernetes = new Kubernetes(config);
```

InspectSample:

``` c#
var namespaces = _kubernetes.ListNamespace();
foreach (var ns in namespaces.Items)
{
    var namespaceName = ns.Metadata.Name;
    Console.WriteLine($"Namespace:{namespaceName}");
    var pods = _kubernetes.ListNamespacedPod(namespaceName);
    foreach (var pod in pods.Items)
    {
        var podName = pod.Metadata.Name;
        Console.WriteLine($"  Pod: {podName}, Labels: {pod.Metadata.Labels.ToJson()}");

        var containers = pod.Spec.Containers;
        foreach (var container in containers)
        {
            Console.WriteLine($"    Container: {container.Name}");
        }
    }
}
```

输出结果如下：

```
Namespace:default
  Pod: reservation, Labels:
    Container: reservation
Namespace:kube-node-lease
Namespace:kube-public
Namespace:kube-system
  Pod: coredns-558bd4d5db-7rdmv, Labels: {"k8s-app":"kube-dns","pod-template-hash":"558bd4d5db"}
    Container: coredns
  Pod: coredns-558bd4d5db-jnxt9, Labels: {"k8s-app":"kube-dns","pod-template-hash":"558bd4d5db"}
    Container: coredns
  Pod: etcd-kind-control-plane, Labels: {"component":"etcd","tier":"control-plane"}
    Container: etcd
  Pod: kindnet-g4xms, Labels: {"app":"kindnet","controller-revision-hash":"5b547684d9","k8s-app":"kindnet","pod-template-generation":"1","tier":"node"}
    Container: kindnet-cni
  Pod: kube-apiserver-kind-control-plane, Labels: {"component":"kube-apiserver","tier":"control-plane"}
    Container: kube-apiserver
  Pod: kube-controller-manager-kind-control-plane, Labels: {"component":"kube-controller-manager","tier":"control-plane"}
    Container: kube-controller-manager
  Pod: kube-proxy-xh7q8, Labels: {"controller-revision-hash":"6bc6858f58","k8s-app":"kube-proxy","pod-template-generation":"1"}
    Container: kube-proxy
  Pod: kube-scheduler-kind-control-plane, Labels: {"component":"kube-scheduler","tier":"control-plane"}
    Container: kube-scheduler
Namespace:local-path-storage
  Pod: local-path-provisioner-547f784dff-prvf5, Labels: {"app":"local-path-provisioner","pod-template-hash":"547f784dff"}
    Container: local-path-provisioner
```

需要注意的是，如果用户没有权限访问所有的命名空间时，遍历命名空间的时候就会报错

## CreatePodSample

上面是一个简单列出 pod 的使用，接着我们来看一个创建 Pod 的示例，我们执行 `kubectl delete po/reservation` 先把之前的 pod 删掉，然后再通过代码创建一个 pod，创建 pod 的代码如下：

``` c#
const string namespaceName = "default";
const string podName = "reservation";
const string containerName = "reservation";
const string image = "weihanli/activityreservation:standalone";

// // try delete pod if exits
// try
// {
//     await _kubernetes.DeleteNamespacedPodAsync(podName, namespaceName);
//     Console.WriteLine($"Pod:{podName} deleted");
// }
// catch
// {
//     //
// }
// await ListPods();

var pod = new V1Pod
{
    Metadata = new V1ObjectMeta
    {
        Name = podName, 
        NamespaceProperty = namespaceName,
        Labels = new Dictionary<string, string>()
        {
            { "app", "reservation" }
        }
    },
    Spec = new V1PodSpec(new List<V1Container>()
    {
        new V1Container(containerName)
        {
            Image = image,
            Ports = new List<V1ContainerPort> {new(80)}
        }
    }),
};
await _kubernetes.CreateNamespacedPodAsync(pod, namespaceName);

await ListPods();

async Task ListPods()
{
    var pods = await _kubernetes.ListNamespacedPodAsync(namespaceName);
    foreach (var item in pods.Items)
    {
        Console.WriteLine($"{item.Metadata.Name}, {item.Metadata.Labels.ToJson()}");
    }
}
```

输出结果如下：

```
reservation, {"app":"reservation"}
```

## Port-Forward Sample

最后来看一下我们的 Port-Forward 的示例，示例代码如下：

首先定义一个通用一点的 Port-Forward 的方法，根据官方给出的示例做了一些改动，更好的支持了 `CancellationToken`：

``` c#
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
```

使用示例如下，使用上面创建的 pod 来演示 port-forward：

``` c#
var pod = (await _kubernetes.ListNamespacedPodAsync("default")).Items
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
    // ignore port-forward exit exception
}
```

输出结果如下：

``` c#
Port-forward started for pod reservation
OK
{"Data":[{"NoticeTitle":"test","NoticeVisitCount":0,"NoticeCustomPath":"test","NoticePublisher":"admin","NoticePublishTime":"2021-04-21T13:29:06.0099785Z"}],"PageNumber":1,"PageSize":10,"TotalCount":1,"PageCount":1,"Count":1}
Port-forward closed
```

## More

通过上面的代码，我们已经可以实现访问 pod 里容器的接口了，只需要将找到 pod 的代码和 port-forward 的代码组合一下就可以达到我们的目标了，对于多个集群可以使用多个配置文件，遍历一下就可以了，如果是在一个配置文件中也可以先获取所有的 cluster，然后在构建 config 的时候指定一个 `currentContext` 就可以了

更多 `KubernetesClient` 使用示例可以参考官方给出的示例：<https://github.com/kubernetes-client/csharp/tree/master/examples>

上面的代码也可以从我的 Github 上获取：<https://github.com/WeihanLi/SamplesInPractice/tree/master/KubernetesClientSample>

## References

- <https://github.com/kubernetes-client/csharp>
- <https://kubernetes.io/docs/reference/using-api/client-libraries/>
- <https://kubernetes.io/docs/reference/using-api/>
- <https://github.com/WeihanLi/SamplesInPractice/tree/master/KubernetesClientSample>

