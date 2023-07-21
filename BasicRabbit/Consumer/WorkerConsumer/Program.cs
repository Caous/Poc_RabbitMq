
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>().AddSingleton<IConsumer, Consumer>();
    })
    .Build();

host.Run();
