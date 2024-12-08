using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = e => ConsoleHelper.WriteLineWithColor(e.ToString(), ConsoleColor.Red);

// InvokeHelper.TryInvoke(JsonSample.MainTest);
// DataAnnotationSample.MainTest();
// TimeProviderSample.MainTest();

// MetricsSample.MainTest();
// InvokeHelper.TryInvoke(KeyedServiceSample.OptionsSample);

// await InvokeHelper.TryInvokeAsync(() => HostedLifecycleServiceSample.MainTest(ApplicationHelper.ExitToken));
// await InvokeHelper.TryInvokeAsync(() => HostedServiceConcurrentSample.MainTest(ApplicationHelper.ExitToken));

// await InvokeHelper.TryInvokeAsync(ConfigureAwaitOptionsSample.MainTest);

// await InvokeHelper.TryInvokeAsync(() => WebAppSlimBuilderSample.MainTest(ApplicationHelper.ExitToken));

// InvokeHelper.TryInvoke(GCSample.MainTest);

// await InvokeHelper.TryInvokeAsync(ParallelSample.MainTest);

// InvokeHelper.TryInvoke(ExceptionThrowSample.MainTest);

// await InvokeHelper.TryInvokeAsync(HttpClientSample.ConfigureHttpClientDefaultsSample);
// Console.WriteLine();
// await InvokeHelper.TryInvokeAsync(HttpClientSample.ConfigureHttpClientDefaultsSample2);

await InvokeHelper.TryInvokeAsync(HttpClientSample.HttpClientGetFromJsonAsAsyncEnumerableSample);

Console.WriteLine("Hello, World!");
