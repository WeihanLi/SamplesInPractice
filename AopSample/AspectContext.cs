using System.Reflection;

namespace AopSample
{
    public class AspectContext
    {
        public MethodInfo MethodInfo { get; set; }

        public object RealTarget { get; set; }

        public object ProxyTarget { get; set; }

        public AbstractAspect[] Aspects { get; set; }
    }
}
