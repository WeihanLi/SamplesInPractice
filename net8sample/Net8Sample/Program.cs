using Net8Sample;
using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException =
    e => ConsoleHelper.InvokeWithConsoleColor(() => Console.WriteLine(e), ConsoleColor.Red);

// JsonSample.MainTest();
// DataAnnotationSample.MainTest();
// TimeProviderSample.MainTest();

// MetricsSample.MainTest();
InvokeHelper.TryInvoke(KeyedServiceSample.MainTest);

Console.WriteLine("Hello, World!");
