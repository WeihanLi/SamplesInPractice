using System.Net;

namespace Net10Samples;

public class IPAddressSample
{
    public static void IsValid()
    {
        Console.WriteLine(IPAddress.IsValid("127.0.0.1"));
        Console.WriteLine(IPAddress.IsValid("localhost"));
    }
}
