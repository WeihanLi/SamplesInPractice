using System;

namespace CSharp9Sample
{
    public class FunctionPointerSample
    {
        public static unsafe void MainTest()
        {
            delegate*<int, int, int> pointer = &Test;
            var result = pointer(1, 1);
            Console.WriteLine(result);
        }

        private static int Test(int num1, int num2)
        {
            Console.WriteLine($"Invoke in {nameof(Test)}, {num1}_{num2}");
            return num1 + num2;
        }
    }
}
