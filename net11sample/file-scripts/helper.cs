#:package WeihanLi.Common@1.0.88
using static WeihanLi.Common.Helpers.ConsoleHelper;

public static class Helper
{
    public static void WriteToConsole(string message)
    {
        Console.WriteLine(message);
    }

    public static void Print(string message)
    {
        WriteLineWithColor(message, ConsoleColor.Green);
    }
}
