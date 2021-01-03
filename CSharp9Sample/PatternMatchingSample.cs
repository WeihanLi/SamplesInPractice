using System;

namespace CSharp9Sample
{
    internal static class PatternMatchingSample
    {
        public static void MainTest()
        {
            var person = new Person();

            //string.IsNullOrEmpty(person.Description), or
            if (person.Description is null or { Length: 0 })
            {
                Console.WriteLine($"{nameof(person.Description)} is IsNullOrEmpty");
            }
            // !string.IsNullOrEmpty(person.Name), and
            if (person.Name is not null and { Length: > 0 })
            {
                if (person.Name[0] is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '.' or ',')
                {
                }
            }
            // not
            if (person.Name is not null)
            {
            }
        }
    }
}
