namespace Net7Sample;
public class ArgumentExceptionSample
{
    public static void MainTest()
    {
        var name = "test";
        ArgumentNullException.ThrowIfNull(name);

        name = null;
        ArgumentNullException.ThrowIfNull(name);
    }
}
