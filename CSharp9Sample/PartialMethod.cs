using System;

namespace CSharp9Sample
{
    partial class PartialMethod
    {
        public static partial void MainTest();

        static partial void Test1();
    }

    partial class PartialMethod
    {
        public static partial void MainTest()
        {
            Test1();
            Console.WriteLine("Partial method works");
        }
    }
}
