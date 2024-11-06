using GetStarted;
using WeihanLi.Common.Helpers;

// load .env variables
dotenv.net.DotEnv.Load();

// await MemorySample.MainTest();
await InvokeHelper.TryInvokeAsync(DotnetConfHelper.MainTest);
// await InvokeHelper.TryInvokeAsync(ArchStoryTeller.Run);
ConsoleHelper.ReadLineWithPrompt();
