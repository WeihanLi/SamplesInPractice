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

            var outputData = string.Empty;

            using(var output = WeihanLi.Common.Helpers.ConsoleOutput.Capture().GetAwaiter().GetResult())
            {
                using var domain = DomainManagement.Random;
                var action = NDelegate
                    .UseDomain(domain)
                    .UseRandomName()
                    .Action(code)
                    ;
                action.Invoke();

                outputData = output.StandardOutput;
            }
            
            Console.WriteLine($"outputData:{outputData}");

            Console.WriteLine("completed.");
            Console.ReadLine();
        }
    }
}
