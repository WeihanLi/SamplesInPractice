using System;

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
    }
}
