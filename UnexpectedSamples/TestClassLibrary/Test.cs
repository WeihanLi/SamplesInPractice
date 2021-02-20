using System;

namespace TestClassLibrary
{
    public class Test
    {
        public static string GetResult()
        {
            var result = string.Empty;

#if NET6_0
            result = "NET6.0";
#elif NET5_0
            result = "NET5.0";
#elif NETCOREAPP3_1
            result = "NETCOREAPP3_1";
#elif NETCOREAPP3_0
            result = "NETCOREAPP3_0";
#elif NETSTANDARD2_1
            result = "NETSTANDARD2_1";
#elif NETSTANDARD2_0
            result = "NETSTANDARD2_0";
#endif

            return result;
        }
    }
}
