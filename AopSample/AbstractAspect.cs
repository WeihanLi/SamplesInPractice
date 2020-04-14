using System;

namespace AopSample
{
    public abstract class AbstractAspect : Attribute
    {
        public abstract void Invoke(MethodInvocationContext methodInvocationContext);
    }

    public class TryInvokeAspect : AbstractAspect
    {
        public override void Invoke(MethodInvocationContext methodInvocationContext)
        {
            try
            {
                methodInvocationContext.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Invoke {methodInvocationContext.MethodInfo.DeclaringType?.FullName}.{methodInvocationContext.MethodInfo.Name} exception");
                Console.WriteLine(e);
            }
        }
    }
}
