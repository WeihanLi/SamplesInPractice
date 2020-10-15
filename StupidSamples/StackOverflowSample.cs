using System;
using System.Threading;

namespace StupidSamples
{
    public class StackOverflowSample
    {
        public static void Test()
        {
            ReadOnlySpan<byte> bytes = stackalloc byte[1024 * 1024];
            Console.WriteLine($"{bytes.Length} passed");

            bytes = stackalloc byte[1024 * 1024 + 1];
            Console.WriteLine($"{bytes.Length} passed");
        }

        public static void Test1()
        {
            Test1();
        }

        public static void Test2()
        {
            var thread = new Thread(() =>
            {
                ReadOnlySpan<byte> bytes = stackalloc byte[31*1024+1];
                Console.WriteLine($"{bytes.Length} passed");

                bytes = stackalloc byte[256*1024+1];
                Console.WriteLine($"{bytes.Length} passed");
            }, 1);
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
