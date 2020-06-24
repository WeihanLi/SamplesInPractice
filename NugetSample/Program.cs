using System;

namespace NugetSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // RawApiSample.Test().Wait();
            NugetClientSdkSample.Test().Wait();

            Console.WriteLine("Completed...");
            Console.ReadLine();
        }
    }
}
