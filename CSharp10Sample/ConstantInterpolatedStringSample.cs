namespace CSharp10Sample;

public class ConstantInterpolatedStringSample
{
    private const string Name = "Alice";
    private const string Hello = $"Hello {Name}";

    private const int Num = 10;

    // Error   CS0133  The expression being assigned to 'ConstantInterpolatedStringSample.HelloNum' must be constant
    //private const string HelloNum = $"Hello {Num}";
    private const string HelloNumNameOf = $"Hello {nameof(Num)}";

    public static void MainTest()
    {
        Console.WriteLine(Hello);
        Console.WriteLine(HelloNumNameOf);
    }
}
