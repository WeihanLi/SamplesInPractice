using System;
using GrpcClientSample;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Logging;
using static System.Console;

LogHelper.ConfigureLogging(x=>x.AddConsole().WithMinimumLevel(LogHelperLogLevel.Info));

await GrpcServiceTest.MainTest();
await HttpServiceTest.MainTest();

WriteLine("Completed...");
ReadLine();