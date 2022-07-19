using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using WeihanLi.Extensions;

namespace KubernetesClientSample
{
    public class CreatePodSample
    {
        private readonly IKubernetes _kubernetes;

        public CreatePodSample(IKubernetes kubernetes)
        {
            _kubernetes = kubernetes;
        }

        public async Task MainTest()
        {
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
            await _kubernetes.CoreV1.CreateNamespacedPodAsync(pod, namespaceName);
            
            await ListPods();

            async Task ListPods()
            {
                var pods = await _kubernetes.CoreV1.ListNamespacedPodAsync(namespaceName);
                foreach (var item in pods.Items)
                {
                    Console.WriteLine($"{item.Metadata.Name}, {item.Metadata.Labels.ToJson()}");
                }
            }
        }
    }
}
