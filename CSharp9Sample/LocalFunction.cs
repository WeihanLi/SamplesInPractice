using System;
using System.Runtime.CompilerServices;

namespace CSharp9Sample
{
    internal static class LocalFunction
    {
        public static void MainTest()
        {
            InnerTest();

            [MethodImpl(MethodImplOptions.Synchronized)]
            void InnerTest()
            {
                Console.WriteLine(nameof(InnerTest));
            }
        }
    }
}
