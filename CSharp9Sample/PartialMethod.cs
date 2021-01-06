using System;

namespace CSharp9Sample
{
    partial class PartialMethod
    {
        public static partial void MainTest();

        static partial void Test1();

        private static partial int Test2();
    }

    partial class PartialMethod
    {
        public static partial void MainTest()
        {
            Test1();
            Test2();
            Console.WriteLine("Partial method works");
        }

        private static partial int Test2() => 1;
    }
}
