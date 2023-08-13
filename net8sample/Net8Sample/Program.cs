using Microsoft.Extensions.Hosting;
using Net8Sample;
using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = e => ConsoleHelper.WriteLineWithColor(e.ToString(), ConsoleColor.Red);
var exitToken = ConsoleHelper.GetExitToken();
// InvokeHelper.TryInvoke(JsonSample.MainTest);
// DataAnnotationSample.MainTest();
// TimeProviderSample.MainTest();

// MetricsSample.MainTest();
// InvokeHelper.TryInvoke(KeyedServiceSample.OptionsSample);

// await InvokeHelper.TryInvokeAsync(() => HostedLifecycleServiceSample.MainTest(exitToken));
await InvokeHelper.TryInvokeAsync(() => HostedServiceConcurrentSample.MainTest(exitToken));

Console.WriteLine("Hello, World!");
