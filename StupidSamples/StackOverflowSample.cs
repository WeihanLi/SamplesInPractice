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
                ReadOnlySpan<byte> bytes = stackalloc byte[16 * 1024 + 1];
                Console.WriteLine($"{bytes.Length} passed");

                bytes = stackalloc byte[32 * 1024 + 1];
                Console.WriteLine($"{bytes.Length} passed");

                bytes = stackalloc byte[256 * 1024 + 1];
                Console.WriteLine($"{bytes.Length} passed");
            }, 1);
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Test3()
        {
            var thread = new Thread(() =>
            {
                TestMethod(1024);
            }, 1);
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Test4()
        {
            var isNegative = true;
            var oper = '>';
            Func<int, bool> filter;

            //// 1.
            //if (!isNegative)
            //{
            //    filter = oper == '>'
            //      ? (i => i > 0)
            //      : (i => i < 0);
            //}
            //else
            //{
            //    filter = oper == '>'
            //      ? (i => i <= 0)
            //      : (i => i >= 0);
            //}

            // 2.
            filter = oper == '>'
                ? (i => i > 0)
                : (i => i < 0);
            if (isNegative)
            {
                filter = i => !filter(i);
            }

            // 3.
            //filter = (isNegative, oper) switch
            //{
            //    (false, '>') => i => i > 0,
            //    (false, _) => i => i < 0,
            //    (true, '>') => i => i <= 0,
            //    (true, _) => i => i >= 0,
            //};

            Console.WriteLine(filter(10));
        }

        private static void TestMethod(int num)
        {
            if (num > 0)
            {
                num--;
                TestMethod(num);
            }
        }
    }
}
