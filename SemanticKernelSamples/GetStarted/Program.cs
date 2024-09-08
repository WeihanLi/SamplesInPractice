// load .env variables

using GetStarted;

dotenv.net.DotEnv.Load();

await MemorySample.MainTest();
// await InvokeHelper.TryInvokeAsync(DotnetConfHelper.MainTest);
