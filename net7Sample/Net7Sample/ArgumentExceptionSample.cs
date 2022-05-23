namespace Net7Sample;
public class ArgumentExceptionSample
{
    public static void MainTest()
    {
        InvokeHelper.OnInvokeException = Console.Error.WriteLine;

        var name = "test";
        ArgumentNullException.ThrowIfNull(name);

        name = null;
        InvokeHelper.TryInvoke(() => ArgumentNullException.ThrowIfNull(name));

        name = string.Empty;
        InvokeHelper.TryInvoke(() => ArgumentException.ThrowIfNullOrEmpty(name));
    }
}
