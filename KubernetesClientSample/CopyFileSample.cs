using k8s;
using System;
using System.Buffers.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KubernetesClientSample;

public class CopyFileSample
{
    private readonly IKubernetes _kubernetes;

    public CopyFileSample(IKubernetes kubernetes)
    {
        _kubernetes = kubernetes;
    }
    
    public async Task MainTest()
    {
        var filePath = @"/app/WeihanLi.Common.dll";
        var @namespace = "default";
        var container = "sparktodo-api";
        var podListResult = await _kubernetes.CoreV1.ListNamespacedPodAsync(@namespace, labelSelector: "app=sparktodo-api");
        var pod = podListResult.Items.FirstOrDefault();
        if (pod is null)
        {
            Console.WriteLine("Pod is not found");
            return;
        }

        Console.WriteLine(pod.Metadata.Name);
        
        var cts = new CancellationTokenSource();
        
        var destDir = Path.Combine(Directory.GetCurrentDirectory(), "tmp");
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        var destFilePath = Path.Combine(destDir, Path.GetFileName(filePath));
        if (File.Exists(destFilePath))
        {
            File.Delete(destFilePath);
        }
        // copy file from pod to local
        var exitCode = await _kubernetes.NamespacedPodExecAsync
        (
            pod.Metadata.Name,
            pod.Metadata.NamespaceProperty,
            container,
            new[]{ "sh", "-c", $"cat {filePath} | base64" },
            true,
            async (_, stdout, _) =>
            {
                using var reader = new StreamReader(stdout);
                var base64 = await reader.ReadToEndAsync();
                var bytes = Convert.FromBase64String(base64);
                
                await using var fs = new FileStream(destFilePath, FileMode.CreateNew);
                await fs.WriteAsync(bytes,0, bytes.Length, cts.Token).ConfigureAwait(false);   
                await fs.FlushAsync(cts.Token);
            },
            cts.Token
        ).ConfigureAwait(false);
        Console.WriteLine($"ExitCode: {exitCode}");

        cts.Cancel();
    }
}
