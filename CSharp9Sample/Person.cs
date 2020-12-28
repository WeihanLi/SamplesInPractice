namespace CSharp9Sample
{
    public class ReadOnlyPerson
    {
        public int Age { get; }

        public string Name { get; }

        public ReadOnlyPerson(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public override string ToString()
        {
            return $"(ReadOnlyPerson)Name:{Name}(Age:{Age})";
        }
    }

    public class Person
    {
        public int Age { get; init; }

        public string Name { get; init; }

        public override string ToString()
        {
            return $"Name:{Name}(Age:{Age})";
        }
    }
}
