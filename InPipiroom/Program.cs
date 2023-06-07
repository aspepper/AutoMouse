using OnPipiroom;
//using System.Runtime.InteropServices;

//[DllImport("user32.dll")]
//static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

//[DllImport("user32.dll")]
//static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<MouseMover>();
    })
    .Build();

//IntPtr hWnd = FindWindow(null, Console.Title);
//if (hWnd != IntPtr.Zero)
//{
//    ShowWindow(hWnd, 0); // Esconde o ícone da barra de tarefas
//}
host.Run();
