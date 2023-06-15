using NonInvasiveKeyboardHookLibrary;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TestCorp
{
    public class MouseMover : BackgroundService
    {
        private uint ScreenResolutionWidth = 0;
        private uint ScreenResolutionHeight = 0;
        private readonly uint WindowsTaskBarTall = 72; // Most tall in Pixels
        private Point CurrentMousePoint = new();
        private bool IsClosed = false;
        private readonly KeyboardHookManager keyboardHookManager = new();

        #region User32 Dynalic Link Library

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        // Framework for getting information about last user input
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        // Native function to get information about last user input
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        #endregion

        public MouseMover()
        {
            GetScreenResolution();
            keyboardHookManager.Start();
            keyboardHookManager.RegisterHotkey(NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt, (int)Keys.C, HotKeyPressed);
        }

        public override void Dispose()
        {
            Console.WriteLine("");
            Console.WriteLine("Service Finished.");
            keyboardHookManager.UnregisterAll();
            keyboardHookManager.Stop();
            base.Dispose();
        }

        void GetScreenResolution()
        {
            ManagementScope scope = new("\\\\.\\ROOT\\cimv2");
            ObjectQuery query = new("SELECT * FROM Win32_VideoController WHERE DeviceID=\"VideoController1\"");
            ManagementObjectSearcher search = new(scope, query);
            ManagementObjectCollection queryCollection = search.Get();
            foreach (ManagementObject m in queryCollection.Cast<ManagementObject>())
            {
                ScreenResolutionWidth = (uint)m["CurrentHorizontalResolution"];
                ScreenResolutionHeight = (uint)m["CurrentVerticalResolution"];
            }
        }

        void LeftClick(Point p)
        {
            Cursor.Position = p;
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            Task.Delay(500, new CancellationToken());
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        void MoveMouse(int toX, int toY, int speed)
        {
            GetCursorPos(ref CurrentMousePoint);
            int deltaX = toX - CurrentMousePoint.X;
            int deltaY = toY - CurrentMousePoint.Y;

            int steps = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));

            float stepX = (float)deltaX / steps;
            float stepY = (float)deltaY / steps;

            float currentX = CurrentMousePoint.X;
            float currentY = CurrentMousePoint.Y;

            Console.Write($"Mouse moverá de ({CurrentMousePoint.X},{CurrentMousePoint.Y}) para ({toX},{toY}), na velocidade {speed} [steps:{steps},stepY:{stepY},stepX:{stepX}]. ");

            for (int i = 0; i <= steps; i++)
            {
                int x = (int)Math.Round(currentX);
                int y = (int)Math.Round(currentY);

                SetCursorPos(x, y);

                Thread.Sleep(speed);
                if ((DateTime.Now - GetLastInputTime()).TotalMilliseconds < 100) { break; }

                currentX += stepX;
                currentY += stepY;
            }
            Console.Write($"-> Out.");
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random speed = new();
            Random rndTime = new();
            Random randomMin = new();
            Random random = new();
            Random timeLater = new();
            int toWait = rndTime.Next(5, 15);

            DateTime timeBegin = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 30, 0)).AddMinutes(timeLater.Next(15, 40));
            DateTime timeToEnd = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 30, 0)).AddMinutes(timeLater.Next(20, 40));
            Console.WriteLine($"Hoje {timeBegin:dd/MM/yyyy} o inicio será às {timeBegin:HH:mm:ss} e o final às {timeToEnd:HH:mm:ss}");
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime lastInteration = GetLastInputTime();
                var currentTime = DateTime.Now;
                if(currentTime.Day != timeBegin.Day)
                { 
                    timeBegin = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 30, 0)).AddMinutes(timeLater.Next(15, 40));
                    timeToEnd = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 30, 0)).AddMinutes(timeLater.Next(20, 40));
                    Console.WriteLine($"Hoje {timeBegin:dd/MM/yyyy} o inicio será às {timeBegin:HH:mm:ss} e o final às {timeToEnd:HH:mm:ss}");
                }
                if (currentTime >= timeBegin && currentTime <= timeToEnd)
                {
                    if ((currentTime - lastInteration).TotalSeconds >= toWait)
                    {
                        toWait = rndTime.Next(5, 15);
                        int x = random.Next((int)ScreenResolutionWidth);
                        int y = random.Next((int)ScreenResolutionHeight);
                        MoveMouse(x, y, speed.Next(1, 3));
                        if (random.Next(20) == 5 && (y > WindowsTaskBarTall && y < (ScreenResolutionHeight - (WindowsTaskBarTall * 2))))
                        { LeftClick(new Point(x, y)); Console.Write(" Clicou!"); }
                    }
                    else
                    { Console.Write($"Tempo não passou ainda, ultimo movimento {lastInteration}, send que se passaram {Math.Round((currentTime - lastInteration).TotalSeconds)}, e a espera é de {toWait}."); }
                    int randomNumber = randomMin.Next(500, 5000);
                    Console.WriteLine($" Aguardando {randomNumber} Milisegundos.");
                    await Task.Delay(randomNumber, stoppingToken);
                }
                else
                {
                    Console.WriteLine("Está fora do horário.");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        static DateTime GetLastInputTime()
        {
            LASTINPUTINFO lastInputInfo = new();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            GetLastInputInfo(ref lastInputInfo);

            return DateTime.Now.AddMilliseconds(-(Environment.TickCount - lastInputInfo.dwTime));
        }

        void HotKeyPressed()
        {
            if (IsClosed) { IsClosed = false; }
            else { IsClosed = true; }
            Console.Write($" -IsClosed:{IsClosed}- ");
        }

    }
}
