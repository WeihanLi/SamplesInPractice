using System.Reflection;

namespace AopSample
{
    public class MethodInvocationContext
    {
        public MethodInfo MethodInfo { get; set; }

        public object Target { get; set; }

        public object[] Parameters { get; set; }

        public object Invoke() => MethodInfo?.Invoke(Target, Parameters);
    }
}
