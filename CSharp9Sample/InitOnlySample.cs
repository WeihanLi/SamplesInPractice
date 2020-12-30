using System;

namespace CSharp9Sample
{
    public class InitOnlySample
    {
        public static void MainTest()
        {
            var p1 = new Person
            {
                Name = "Michael",
                Age = 10
            };

            // compiler error
            //p1.Age = 12;

            Console.WriteLine(p1);

            Person p2 = new()
            {
                Name = "Jane",
                Age = 10,
            }, p3 = new()
            {
                Name = "Alice"
            };
            Console.WriteLine(p2);
            Console.WriteLine(p3);

            ReadOnlyPerson p4 = new("Tom", 10);
            Console.WriteLine(p4);

            var model = new TestInitModel() { Name = "Test" };
        }

        private class TestInitModel
        {
            private readonly string _name;

            public string Name
            {
                get => _name;
                init => _name = value;
            }
        }
    }
}
