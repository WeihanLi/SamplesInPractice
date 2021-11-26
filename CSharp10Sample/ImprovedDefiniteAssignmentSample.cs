namespace CSharp10Sample;

// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/improved-definite-assignment
// https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#improved-definite-assignment
public class ImprovedDefiniteAssignmentSample
{
    public static void MainTest()
    {
        var c = new C();
        if (c != null && c.M(out var obj0))
        {
            Console.WriteLine(obj0.ToString());
        }
    }

    private class C
    {
        public bool M(out object obj)
        {
            obj = new object();
            return true;
        }
    }
}
