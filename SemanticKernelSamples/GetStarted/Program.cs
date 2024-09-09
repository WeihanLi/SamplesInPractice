// load .env variables

using GetStarted;
using WeihanLi.Common.Helpers;

dotenv.net.DotEnv.Load();

// await MemorySample.MainTest();
await InvokeHelper.TryInvokeAsync(DotnetConfHelper.MainTest);
