using System;
using WeihanLi.Common.Helpers;

namespace NPOISample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // RawNPOISample.BasicTest();
            // RawNPOISample.PrepareWorkbookTest();
            // RawNPOISample.ImportDataTest();

            InvokeHelper.OnInvokeException = Console.WriteLine;
            InvokeHelper.TryInvoke(NPOIExtensionSample.MainTest);

            Console.WriteLine("Completed!");
            Console.ReadLine();
        }
    }
}
