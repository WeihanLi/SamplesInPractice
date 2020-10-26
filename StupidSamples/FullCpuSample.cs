using System;
using System.Threading.Tasks;

namespace StupidSamples
{
    public class FullCpuSample
    {
        public static void Test()
        {
            Parallel.For(0,
                Environment.ProcessorCount,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                i =>
                {
                    while (true)
                    {
                    }
                });
        }
    }
}
