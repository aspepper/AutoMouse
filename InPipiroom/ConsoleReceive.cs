using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace InPipiroom
{
    public class ConsoleReceiver : Form, IDisposable
    {


        bool IsHided = true;
        const int MYACTION_HOTKEY_ID = 1;

        [DllImport("user32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        readonly IntPtr handle = GetConsoleWindow();

        public ConsoleReceiver()
        {
            SetFormProperties();
            RegisterHotKey(handle, MYACTION_HOTKEY_ID, 6, (int)Keys.F1);
            IsHided = true;
            ShowWindow(handle, SW_HIDE);
        }

        private void SetFormProperties()
        {
            Text = "InPipiroom";
            Width = 0;
            Height = 0;
            ShowIcon = false;
            ShowInTaskbar = false;
            Opacity = 0.0;
            Visible = false;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                if (IsHided) { ShowWindow(handle, SW_SHOW); IsHided = false; }
                else { ShowWindow(handle, SW_HIDE); IsHided = true; }
            }
            base.WndProc(ref m);
        }

        public void Disposable(object sender, EventArgs e)
        {
            UnregisterHotKey(handle, MYACTION_HOTKEY_ID);
        }
    }

}
