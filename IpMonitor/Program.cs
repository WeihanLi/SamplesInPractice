using IpMonitor;

Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<INotification, GoogleChatNotification>();
    })
    .Build()
    .Run();
