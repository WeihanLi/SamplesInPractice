using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CSharp12Sample;

public static class ExperimentalSample
{
    public static void MainTest()
    {
        Helper.Test();
    }
}

file sealed class Helper
{
    [Experimental("EXP001")]
    public static void Test([CallerMemberName]string callerMemberName = "")
    {
        Console.WriteLine(callerMemberName);
    }
}
