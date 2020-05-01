using System;

namespace FluentAspectSample
{
    public interface ISvc1
    {
        void Invoke();
    }

    public interface ISvc2
    {
        void Invoke();
    }

    public class Svc2 : ISvc2
    {
        public void Invoke()
        {
            Console.WriteLine($"invoking in {GetType().Name} ...");
        }

        public void Invoke2()
        {
            Console.WriteLine($"invoking in {GetType().Name} ...");
        }
    }

    public class Svc3
    {
        public virtual void Invoke()
        {
            Console.WriteLine($"invoking in {GetType().Name} ...");
        }
    }

    public class Svc4
    {
        public virtual void Invoke()
        {
            Console.WriteLine($"invoking in {GetType().Name} ...");
        }

        public void Invoke2()
        {
            Console.WriteLine($"invoking2 in {GetType().Name} ...");
        }

        public virtual void Invoke3()
        {
            Console.WriteLine($"invoking3 in {GetType().Name} ...");
        }
    }
}
