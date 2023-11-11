using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CSharp12Sample;

public static class ExperimentalSample
{
    public static void MainTest()
    {
        Helper.Test();
        
        Helper.Test2();
    }
}

file sealed class Helper
{
    [Experimental("EXP001")]
    public static void Test([CallerMemberName]string callerMemberName = "")
    {
        Console.WriteLine(callerMemberName);
    }
    
    [Experimental("EXP002", UrlFormat = "https://diagnostic.weihan.xyz/{0}")]
    public static void Test2([CallerMemberName]string callerMemberName = "")
    {
        Console.WriteLine(callerMemberName);
    }
}
