using System;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;

namespace HttpClientTest
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        public static async Task MainAsync(string[] args)
        {
            InvokeHelper.OnInvokeException = Console.WriteLine;

            await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.FormUrlEncodedContentLengthTest);
            Console.WriteLine();
            await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.StringContentLengthTest);
            Console.WriteLine();
            await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.ByteArrayContentLengthTest);

            Console.WriteLine("Completed!");
            Console.ReadLine();
        }
    }
}
