using HttpClientTest;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Logging;

LogHelper.ConfigureLogging(builder => builder.AddConsole());

// await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.FormUrlEncodedContentLengthTest);
// Console.WriteLine();
// await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.StringContentLengthTest);
// Console.WriteLine();
// await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.ByteArrayContentLengthTest);
// Console.WriteLine();

// await InvokeHelper.TryInvokeAsync(NoAutoRedirectSample.MainTest);

await InvokeHelper.TryInvokeAsync(HttpClientEventSample.MainTest);

Console.WriteLine("Completed!");
Console.ReadLine();
