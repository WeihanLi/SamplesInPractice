using System;
using UnexpectedSamples;
using WeihanLi.Common.Helpers;

InvokeHelper.OnInvokeException = Console.WriteLine;
InvokeHelper.TryInvoke(EnumParseSample.MainTest);

Console.WriteLine("Hello World!");
Console.ReadLine();
