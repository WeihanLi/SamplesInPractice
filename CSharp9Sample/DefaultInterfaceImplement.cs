using System;
using WeihanLi.Extensions;

namespace CSharp9Sample
{
    internal interface IFlyH
    {
    }

    internal interface IFly
    {
        private const string DefaultName = nameof(IFly);

        protected static string GetDefaultName() => DefaultName;

        public static string GetPublicName() => DefaultName;
        
        // Interface cannot contain instance fields
        // private string name = "";
        
        string Name { get; }

        void Fly() => Console.WriteLine($"{Name.GetValueOrDefault((DefaultName))} is flying");
    }

    internal class Superman : IFly
    {
        public string Name => nameof(Superman);
    }

    internal class MonkeyKing : IFly
    {
        public string Name => nameof(MonkeyKing);

        public void Fly()
        {
            Console.WriteLine($"I'm {Name}, I'm flying, DefaultName:{IFly.GetDefaultName()}");
        }
    }

    public class DefaultInterfaceImplement
    {
        public static void MainTest()
        {
            IFly fly = new Superman();
            fly.Fly();

            fly = new MonkeyKing();
            fly.Fly();
            
            // Cannot access protected method 'GetDefaultName' here
            // IFly.GetDefaultName().Dump();
            
            IFly.GetPublicName().Dump();
        }
    }
}
