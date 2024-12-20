using GetStarted;
using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = ex => ConsoleHelper.ErrorWriteLineWithColor(ex.ToString(), ConsoleColor.Red);

// load .env variables
dotenv.net.DotEnv.Load();

// await MemorySample.MainTest();
// await InvokeHelper.TryInvokeAsync(DotnetConfHelper.MainTest);
await InvokeHelper.TryInvokeAsync(ArchStoryTeller.Run);
// await InvokeHelper.TryInvokeAsync(ArchStoryTeller.CreateDraftTest);
// await InvokeHelper.TryInvokeAsync(ArchStoryTeller.JsonContentTest);

ConsoleHelper.ReadLineWithPrompt();
