using System.Runtime.Versioning;

namespace CSharp10Sample;

[RequiresPreviewFeatures]
public class InterfaceStaticMember
{
    private interface IAnimal
    {
        static abstract string GetName();

        static void Test()
        {
            Console.WriteLine("Test");
        }
    }

    private class Cat : IAnimal
    {
        public static string GetName()
        {
            return nameof(Cat);
        }
    }

    private class Dog : IAnimal
    {
        public static string GetName()
        {
            return nameof(Dog);
        }
    }

    public static void MainTest()
    {
        IAnimal.Test();
        Console.WriteLine(Cat.GetName());
        Console.WriteLine(Dog.GetName());
    }
}
