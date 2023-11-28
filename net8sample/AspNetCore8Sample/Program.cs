using WeihanLi.Extensions;

Console.WriteLine(Enum.GetNames<Level>().StringJoin(","));

// await RequestTimeoutsSample.MainTest(args);
// await BasicSetupSample.MainTest(args);
// await IdentityApiSample.MainTest(args);
// await HttpLoggingInterceptorSample.MainTest(args);
await ExceptionHandlerSample.MainTest(args);
