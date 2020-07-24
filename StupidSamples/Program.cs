using System;

namespace StupidSamples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // dead lock
            // DeadLockSample.Test();

            // full CPU
            // FullCpuSample.Test();

            // full memory
            FullMemorySample.Test();

            Console.WriteLine("completed");
            Console.ReadLine();
        }
    }
}
