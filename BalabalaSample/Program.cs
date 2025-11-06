using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = ex => ConsoleHelper.WriteLineWithColor(ex.ToString(), ConsoleColor.Red);

// ListForEachSample.MainTest();
// await ListForEachSample.MainTestAsync();

// await CorrelationIdSampleV2.MainTest();

// InvokeHelper.TryInvoke(CorrelationIdSampleV3.MainTest);

// ParamsSample.MainTest();

await InvokeHelper.TryInvokeAsync(DotnetConfAgendaCrawler.MainTest);
// await InvokeHelper.TryInvokeAsync(DotnetConfAgendaAnalyzer.RunAsync);

// await InvokeHelper.TryInvokeAsync(BlogProductSample.RunAsync);

// await RenamingTool.MainAsync();

ConsoleHelper.ReadKeyWithPrompt();
