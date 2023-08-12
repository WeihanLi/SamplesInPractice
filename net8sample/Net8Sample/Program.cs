using Net8Sample;
using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = e => ConsoleHelper.WriteLineWithColor(e.ToString(), ConsoleColor.Red);
var exitToken = ConsoleHelper.GetExitToken();
// JsonSample.MainTest();
// DataAnnotationSample.MainTest();
// TimeProviderSample.MainTest();

// MetricsSample.MainTest();
// InvokeHelper.TryInvoke(KeyedServiceSample.OptionsSample);

await InvokeHelper.TryInvokeAsync(() => HostedLifecycleServiceSample.MainTest(exitToken));

Console.WriteLine("Hello, World!");
