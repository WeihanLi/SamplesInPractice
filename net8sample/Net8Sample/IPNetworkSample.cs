// https://github.com/dotnet/runtime/issues/79946
// https://github.com/dotnet/runtime/pull/82779
// https://learn.microsoft.com/en-us/dotnet/api/system.net.ipnetwork?view=net-8.0
using System.Net;

namespace Net8Sample;

public static class IPNetworkSample
{
    public static void MainTest()
    {
        var ipNetwork = "198.51.0.0/16";
        var network = IPNetwork.Parse(ipNetwork);
        var ip = IPAddress.Parse("198.51.250.42");
        Console.WriteLine(network.Contains(ip));
    }
}
