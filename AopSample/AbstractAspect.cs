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
            Console.WriteLine("begin ...");
            try
            {
                methodInvocationContext.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Invoke {methodInvocationContext.Method.DeclaringType?.FullName}.{methodInvocationContext.Method.Name} exception");
                Console.WriteLine(e);
            }
            Console.WriteLine("end ...");
        }
    }
}
