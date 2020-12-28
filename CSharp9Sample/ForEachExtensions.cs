using System;
using System.Collections.Generic;

namespace CSharp9Sample
{
    public static class ForEachExtensions
    {
        private static IEnumerator<char> GetEnumerator(this int num)
        {
            return num.ToString().GetEnumerator();
        }

        public static void MainTest()
        {
            //var enumerable = Enumerable.Range(1, 10).ToArray();
            //foreach (var i in enumerable)
            //{
            //    Console.WriteLine(i);
            //}

            //var enumerator = enumerable.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    Console.WriteLine(enumerator.Current);
            //}

            var num = 123;
            foreach (var i in num)
            {
                Console.WriteLine(i);
            }
        }
    }
}
