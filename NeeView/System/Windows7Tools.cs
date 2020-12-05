using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Windows7専用
    /// </summary>
    public static class Windows7Tools
    {
        #region NativeApi

        internal static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string className, string windowTitle);

            [DllImport("user32.dll")]
            public static extern int SetForegroundWindow(IntPtr hwnd);
        }

        #endregion


        public static bool IsWindows7 => System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1;


        public static void RecoveryTaskBar(Window _window)
        {
            if (!IsWindows7 || _window.WindowState != WindowState.Normal) return;

            IntPtr hTaskbarWnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            NativeMethods.SetForegroundWindow(hTaskbarWnd);
            _window.Activate();
        }
    }

}
