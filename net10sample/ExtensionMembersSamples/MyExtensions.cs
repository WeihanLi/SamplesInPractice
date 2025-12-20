namespace System;

public static class MyExtensions
{
    public static string CustomHexStringLower(this byte[] bytes)
    {
#if NET9_0_OR_GREATER
        return Convert.ToHexStringLower(bytes);
#else
        return Convert.ToHexString(bytes).ToLowerInvariant();
#endif
    }

#if !NET9_0_OR_GREATER
    extension(Convert)
    {
        public static string ToHexStringLower(byte[] bytes)
        {
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
#endif
}
