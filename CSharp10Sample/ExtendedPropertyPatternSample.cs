namespace CSharp10Sample;

public class ExtendedPropertyPatternSample
{
    public static void MainTest()
    {
        var a = new TestModelA(new TestModelB(new TestModelC("C"), "B"), "A");
        if (a is { B.C.Name.Length: > 0 })
        {
            Console.WriteLine(a.B.C.Name);
        }
    }

    private record TestModelA(TestModelB B, string Name);

    private record TestModelB(TestModelC C, string Name);

    private record TestModelC(string Name);
}
