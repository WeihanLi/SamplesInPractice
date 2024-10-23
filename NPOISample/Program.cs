using System;
using WeihanLi.Common.Helpers;
using NPOISample;

// RawNPOISample.BasicTest();
// RawNPOISample.PrepareWorkbookTest();
// RawNPOISample.ImportDataTest();

InvokeHelper.OnInvokeException = Console.WriteLine;
// InvokeHelper.TryInvoke(NPOIImageSample.MainTest);
InvokeHelper.TryInvoke(MultiSheetsSample.MainTest);

Console.WriteLine("Completed!");
Console.ReadLine();
