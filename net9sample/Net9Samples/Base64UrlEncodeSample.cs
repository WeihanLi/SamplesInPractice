using System.Buffers.Text;
using System.Text;
using WeihanLi.Common.Helpers;

namespace Net9Samples;

public static class Base64UrlEncodeSample
{
    public static void MainTest()
    {
        var encodedBase64UrlText = Base64Url.EncodeToString("Hello"u8);
        Console.WriteLine(encodedBase64UrlText);

        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ_";
        foreach (var item in token.Split('.'))
        {
            var base64Valid = Base64.IsValid(item);
            var base64UrlValid = Base64Url.IsValid(item);
            Console.WriteLine($"{nameof(base64Valid)}: {base64Valid}");
            Console.WriteLine($"{nameof(base64UrlValid)}: {base64UrlValid}");

            try
            {
                var decodedBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(item));
                Console.WriteLine($"{nameof(decodedBase64)}: {decodedBase64}");
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteLineWithColor(e.ToString(), ConsoleColor.Red);
            }

            try
            {
                var decodedBase64Url = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(item));
                Console.WriteLine($"{nameof(decodedBase64Url)}: {decodedBase64Url}");
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteLineWithColor(e.ToString(), ConsoleColor.Red);
            }
        }
    }
}
