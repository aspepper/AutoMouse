using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OnPipiroom
{
    public class MouseMover : BackgroundService
    {

        private uint ScreenResolutionWidth = 0;
        private uint ScreenResolutionHeight = 0;
        private readonly uint WindowsTaskBarTall = 72; // Most tall in Pixels

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

        #region User32 Dynalic Link Library

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

        static void LeftClick(Point p)
        {
            Cursor.Position = p;
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            Task.Delay(500, new CancellationToken());
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random randomMin = new();
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentTime = DateTime.Now;
                if (currentTime.Hour >= 8 && currentTime.Hour <= 20)
                {
                    if ((DateTime.Now - GetLastInputTime()).TotalSeconds >= 15)
                    {
                        Random random = new();
                        int x = random.Next((int)ScreenResolutionWidth);
                        int y = random.Next((int)ScreenResolutionHeight);
                        SetCursorPos(x, y);
                        if (random.Next(10) == 5 && (y > WindowsTaskBarTall && y < (ScreenResolutionHeight - WindowsTaskBarTall)))
                        { LeftClick(new Point(x, y)); }
                    }
                    int randomNumber = randomMin.Next(500, 5000);
                    await Task.Delay(randomNumber, stoppingToken);
                }
                else
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private static DateTime GetLastInputTime()
        {
            LASTINPUTINFO lastInputInfo = new();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            GetLastInputInfo(ref lastInputInfo);

            return DateTime.Now.AddMilliseconds(-(Environment.TickCount - lastInputInfo.dwTime));
        }

    }
}
