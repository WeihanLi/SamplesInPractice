using System;
using k8s;
using WeihanLi.Extensions;

namespace KubernetesClientSample
{
    public class InspectSample
    {
        private readonly IKubernetes _kubernetes;

        public InspectSample(IKubernetes kubernetes)
        {
            _kubernetes = kubernetes;
        }
        public void MainTest()
        {
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
        }
    }
}
