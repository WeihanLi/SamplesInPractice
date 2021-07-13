using System;
using HttpClientTest;
using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = Console.WriteLine;

// await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.FormUrlEncodedContentLengthTest);
// Console.WriteLine();
// await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.StringContentLengthTest);
// Console.WriteLine();
// await InvokeHelper.TryInvokeAsync(FormUrlEncodeContentTest.ByteArrayContentLengthTest);
// Console.WriteLine();

await InvokeHelper.TryInvokeAsync(NoAutoRedirectSample.MainTest);

Console.WriteLine("Completed!");
Console.ReadLine();
