using System;
using System.Threading;
using WeihanLi.Totp;

namespace ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var otp = new Totp(OtpHashAlgorithm.SHA1, 4);
            var secretKey = "12345678901234567890";
            var output = otp.Compute(secretKey);
            Console.WriteLine($"output: {output}");
            Thread.Sleep(1000 * 30);
            var verifyResult = otp.Verify(secretKey, output);
            Console.WriteLine($"Verify result: {verifyResult}");
            verifyResult = otp.Verify(secretKey, output, TimeSpan.FromSeconds(60));
            Console.WriteLine($"Verify result: {verifyResult}");

            Console.ReadLine();
        }
    }
}
