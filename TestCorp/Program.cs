using InPipiroom;
using OnPipiroom;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows.Forms;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<MouseMover>();
    })
    .Build();

host.Run();
//Application.Run(new ConsoleReceiver());
