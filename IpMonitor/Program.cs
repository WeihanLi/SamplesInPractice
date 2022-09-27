using IpMonitor;

Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<INotification, GoogleChatNotification>();
    })
#if !DEBUG
    // https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service
    .UseWindowsService(options =>
    {
        options.ServiceName = "IpMonitor";
    })
#endif
    .Build()
    .Run();
