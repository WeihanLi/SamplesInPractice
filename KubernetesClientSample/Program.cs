using System;
using k8s;
using KubernetesClientSample;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
        .RegisterAssemblyTypes(t => t.Name.EndsWith("Sample"), ServiceLifetime.Transient)
        .AddSingleton<IKubernetes>(_ => new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()))
        .BuildServiceProvider();

// services.GetRequiredService<InspectSample>().MainTest();
// await services.GetRequiredService<CreatePodSample>().MainTest();
// await services.GetRequiredService<PortForwardSample>().MainTest();
await services.GetRequiredService<CopyFileSample>().MainTest();

Console.WriteLine("Completed");
Console.ReadLine();
