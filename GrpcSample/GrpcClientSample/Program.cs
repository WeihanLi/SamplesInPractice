using System;
using GrpcClientSample;
using WeihanLi.Common.Helpers;
using static System.Console;

InvokeHelper.OnInvokeException = ex =>
{
    var originalColor = ForegroundColor;
    ForegroundColor = ConsoleColor.Red;
    WriteLine(ex);
    ForegroundColor = originalColor;
};

await GrpcServiceTest.MainTest();
await HttpServiceTest.MainTest();

WriteLine("Completed...");
