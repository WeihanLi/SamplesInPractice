using System;

namespace CSharp9Sample
{
    public class InitOnlySample
    {
        public static void MainTest()
        {
            Person p1 = new()
            {
                Name = "Michael",
                Age = 10
            };
            Console.WriteLine(p1);

            var p2 = new Person()
            {
                Name = "Jane",
                Age = 10,
            };
            // compiler error
            // p1.Age = 12;
            Console.WriteLine(p2);

            ReadOnlyPerson p3 = new("Tom", 10);
            Console.WriteLine(p3);
        }
    }
}
