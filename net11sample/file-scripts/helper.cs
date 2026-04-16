#:package WeihanLi.Common@1.0.87
using static WeihanLi.Common.Helpers.ConsoleHelper;

public static class Helper
{
    public static void Print(string message)
    {
        WriteLineWithColor(message, ConsoleColor.Green);
    }
}
