using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = e => ConsoleHelper.WriteLineWithColor(e.ToString(), ConsoleColor.Red);

var exitToken = InvokeHelper.GetExitToken();
// InvokeHelper.TryInvoke(JsonSample.MainTest);
// DataAnnotationSample.MainTest();
// TimeProviderSample.MainTest();

// MetricsSample.MainTest();
// InvokeHelper.TryInvoke(KeyedServiceSample.OptionsSample);

// await InvokeHelper.TryInvokeAsync(() => HostedLifecycleServiceSample.MainTest(exitToken));
// await InvokeHelper.TryInvokeAsync(() => HostedServiceConcurrentSample.MainTest(exitToken));

// await InvokeHelper.TryInvokeAsync(ConfigureAwaitOptionsSample.MainTest);

// await InvokeHelper.TryInvokeAsync(() => WebAppSlimBuilderSample.MainTest(exitToken));

// InvokeHelper.TryInvoke(GCSample.MainTest);

// await InvokeHelper.TryInvokeAsync(ParallelSample.MainTest);

InvokeHelper.TryInvoke(ExceptionThrowSample.MainTest);

Console.WriteLine("Hello, World!");
