using NetHook.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NetHook.UI.Extensions
{
    public static class ControlHelper
    {
        [DllImport("user32.dll", EntryPoint = "SendMessageA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private const int WM_SETREDRAW = 0xB;

        public static void SuspendDrawing(this Control target)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 0, 0);
        }

        public static IDisposable CreateUpdateContext(this Control control)
        {
            control.SuspendDrawing();
            return new DisposeAction<Control>(control, (x) => x.ResumeDrawing());
        }

        public static void ResumeDrawing(this Control target)
        {
            ResumeDrawing(target, true);
        }

        public static void ResumeDrawing(this Control target, bool redraw)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 1, 0);

            if (redraw)
            {
                target.Refresh();
            }
        }

        public static void Invoke(this Control control, Action action)
        {
            if (control.Disposing)
                return;

            try
            {
                if (control.InvokeRequired)
                    control.Invoke(new Action(() =>
                    {
                        if (control.Disposing)
                            return;
                        action();
                    }));
                else
                    action();
            }
            catch (ObjectDisposedException) { }
        }
    }
}
