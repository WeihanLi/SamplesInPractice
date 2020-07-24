using System;
using System.Threading.Tasks;

namespace StupidSamples
{
    public class FullCpuSample
    {
        public static void Test()
        {
            Parallel.For(0, Environment.ProcessorCount, i =>
              {
                  while (true)
                  {
                  }
              });
        }
    }
}
