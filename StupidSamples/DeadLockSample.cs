using System;
using System.Threading.Tasks;

namespace StupidSamples
{
    public class DeadLockSample
    {
        private static readonly object Lock = new object();

        public static void Test()
        {
            lock (Lock)
            {
                Task.Run(TestMethod1).Wait();
            }
        }

        private static void TestMethod1()
        {
            lock (Lock)
            {
                Console.WriteLine("xxx");
            }
        }
    }
}
