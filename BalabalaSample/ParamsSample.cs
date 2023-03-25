using WeihanLi.Common.Helpers;

namespace BalabalaSample;

public class ParamsSample
{
    public static void MainTest()
    {
        ParamsTest();
        Console.ReadLine();
        StringFormatTest();
    }

    private static void StringFormatTest()
    {
        var format = "{0} 123 {1}";
        InvokeHelper.TryInvoke(() =>
        {
            var str = string.Format(format, 1, 2);
            Console.WriteLine(str);
        });

        InvokeHelper.TryInvoke(() =>
        {
            var str = string.Format(format, new[] { "1", "2" });
            Console.WriteLine(str);
        });

        InvokeHelper.TryInvoke(() =>
        {
            var str = string.Format(format, new[] { 1, 2 });
            Console.WriteLine(str);
        });

        InvokeHelper.TryInvoke(() =>
        {
            var str = string.Format(format, new[] { 1, 2 }.Cast<object>().ToArray());
            Console.WriteLine(str);
        });

        InvokeHelper.TryInvoke(() =>
        {
            var str = string.Format(format, new object[] { 1, 2 });
            Console.WriteLine(str);
        });
    }

    private static void ParamsTest()
    {
        ParamsMethod(1, 2, 3);
        ParamsMethod(new[] { 1, 2, 3 });
        ParamsMethod(new[] { "1", "2", "3" });
        ParamsMethod(new object[] { 1, 2, 3 });
    }

    private static void ParamsMethod(params object[] args)
    {
        if (args is null)
        {
            Console.WriteLine("Empty");
            return;
        }

        Console.WriteLine(args.Length);
    }
}
