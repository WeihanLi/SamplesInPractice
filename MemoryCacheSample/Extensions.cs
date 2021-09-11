namespace System;

static class Extensions
{
    public static void Dump(this List<int> values)
    {
        var value = string.Join(",", values);
        Console.WriteLine(value);
    }
}
