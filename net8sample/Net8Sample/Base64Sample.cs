using System.Buffers.Text;
using WeihanLi.Extensions;

namespace Net8Sample;

public class Base64Sample
{
    public static void MainTest()
    {
        var hello = "Hello";
        Console.WriteLine(Base64.IsValid(hello));
        Console.WriteLine(Base64.IsValid(Convert.ToBase64String(hello.GetBytes())));
    }
}
