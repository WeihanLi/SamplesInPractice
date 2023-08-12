using Net8Sample;
using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException =
    e => ConsoleHelper.InvokeWithConsoleColor(() => Console.WriteLine(e), ConsoleColor.Red);

// JsonSample.MainTest();
// DataAnnotationSample.MainTest();
// TimeProviderSample.MainTest();

// MetricsSample.MainTest();
InvokeHelper.TryInvoke(KeyedServiceSample.OptionsSample);
// await InvokeHelper.TryInvokeAsync(KeyedServiceSample.WebApiSample);

ConsoleHelper.ReadLineWithPrompt();
Console.WriteLine("Hello, World!");
