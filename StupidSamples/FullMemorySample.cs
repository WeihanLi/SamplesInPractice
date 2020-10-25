using System;
using System.Collections.Generic;

namespace StupidSamples
{
    public class FullMemorySample
    {
        public static void Test()
        {
            Console.ReadLine();
            var bytes = GC.GetTotalAllocatedBytes();
            Console.WriteLine($"AllocatedBytes: { bytes } bytes");
            var list = new List<byte[]>();
            try
            {
                while (true)
                {
                    list.Add(new byte[85000]);
                }
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine(nameof(OutOfMemoryException));
                Console.WriteLine(list.Count);
                bytes = GC.GetTotalAllocatedBytes();
                Console.WriteLine($"AllocatedBytes: { bytes } bytes");
            }
            Console.ReadLine();
        }
    }
}
