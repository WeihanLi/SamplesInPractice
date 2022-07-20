using k8s;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        var exitCode = await CopyFileFromPod(_kubernetes, pod.Metadata.Name, pod.Metadata.NamespaceProperty, container,
            filePath, destFilePath, cts.Token);
        Console.WriteLine($"CopyFileFromPod ExitCode: {exitCode}");

        // copy file from local to pod
        var exitCode1 = await CopyFileToPod(_kubernetes, pod.Metadata.Name, pod.Metadata.NamespaceProperty, container,
            @"C:\projects\sources\dotnet-exec\tests\IntegrationTest\CodeSamples\DumpAssemblyInfoSample.cs", 
            "/app/DumpAssemblyInfoSample.cs", cts.Token);
        Console.WriteLine($"CopyFileToPod exitCode: {exitCode1}");

        cts.Cancel();
    }
    
    public static async Task<int> CopyFileFromPod(IKubernetes kubernetes,
        string pod,
        string @namespace,
        string container,
        string srcPath,
        string destPath,
        CancellationToken cancellationToken = default)
    {
        var exitCode = await kubernetes.NamespacedPodExecAsync
        (
            pod,
            @namespace,
            container,
            new[]{ "sh", "-c", $"cat {srcPath} | base64" },
            true,
            async (_, stdout, _) =>
            {
                // using var reader = new StreamReader(stdout);
                // var base64 = await reader.ReadToEndAsync();
                // var bytes = Convert.FromBase64String(base64);

                // https://stackoverflow.com/questions/12901705/decoding-base64-stream-to-image
                await using var stream = new CryptoStream(stdout, new FromBase64Transform(), CryptoStreamMode.Read);
                
                await using var fs = new FileStream(destPath, FileMode.Create);
                await stream.CopyToAsync(fs, cancellationToken);
                
                await fs.FlushAsync(cancellationToken);
            },
            cancellationToken
        ).ConfigureAwait(false);
        return exitCode;
    }

    public static async Task<int> CopyFileToPod(IKubernetes kubernetes, 
        string pod,
        string @namespace,
        string container,
        string srcPath,
        string destPath,
        CancellationToken cancellationToken = default)
    {
        var fileBytes = await File.ReadAllBytesAsync(srcPath, cancellationToken);
        var base64String = Convert.ToBase64String(fileBytes);
        var exitCode = await kubernetes.NamespacedPodExecAsync
        (
            pod,
            @namespace,
            container,
            new[]{ "sh", "-c", $"echo {base64String} | base64 --decode > {destPath}" },
            true,
            (_, _, _) => Task.CompletedTask,
            cancellationToken
        ).ConfigureAwait(false);
        return exitCode;
    }
}
