using System;
using Natasha.CSharp;

namespace NatashaSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var code = "Console.WriteLine(\"Hello World!\");";

            AssemblyDomain.Init();
            using var domain = DomainManagement.Random;
            var action = NDelegate
                .UseDomain(domain)
                .UseRandomName()
                .Action(code)
                ;
            action.Invoke();

            Console.ReadLine();
        }
    }
}
