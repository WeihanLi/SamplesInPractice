using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = ex =>
{
    ConsoleHelper.InvokeWithConsoleColor(() => Console.WriteLine(ex), ConsoleColor.Red);
};

// ListForEachSample.MainTest();
// await ListForEachSample.MainTestAsync();

await CorrelationIdSampleV2.MainTest();

// ParamsSample.MainTest();

Console.ReadLine();
