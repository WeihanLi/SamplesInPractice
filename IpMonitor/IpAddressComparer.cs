using System.Net;
using WeihanLi.Extensions;

namespace IpMonitor;

public sealed class IpAddressComparer: IComparer<IPAddress>
{
    public int Compare(IPAddress? x, IPAddress? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;

        var bytes1 = x.MapToIPv4().ToString().SplitArray<byte>(new []{ '.' });
        var bytes2 = y.MapToIPv4().ToString().SplitArray<byte>(new []{ '.' });
        for (var i = 0; i < bytes1.Length; i++)
        {
            if (bytes1[i] != bytes2[i])
            {
                return bytes1[i].CompareTo(bytes2[i]);
            }
        }
        
        return 0;
    }
}
