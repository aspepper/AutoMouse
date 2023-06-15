using TestCorp;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<MouseMover>();
    })
    .Build();

host.Run();

