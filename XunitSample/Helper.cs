using System;

namespace XunitSample
{
    internal class Helper
    {
        public static int Add(int x, int y)
        {
            return x + y;
        }

        public static void ArgumentExceptionTest() => throw new ArgumentException();

        public static void ArgumentNullExceptionTest() => throw new ArgumentNullException();
    }
}
