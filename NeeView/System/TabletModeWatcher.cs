using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    /// <summary>
    /// タブレットモード判定 (Windows10)
    /// </summary>
    public class TabletModeWatcher
    {
        static TabletModeWatcher() => Current = new TabletModeWatcher();
        public static TabletModeWatcher Current { get; }


        private const int WM_SETTINGCHANGE = 0x001A;

        private Window _window;
        private bool _isTabletMode = false;
        private int _dartyValue = 1;

        public TabletModeWatcher()
        {
        }

        public void Initialize(Window window)
        {
            if (_window != null) throw new InvalidOperationException();

            var hsrc = HwndSource.FromVisual(window) as HwndSource;
            _window = window;

            hsrc.AddHook(WndProc);
        }

        public bool IsTabletMode
        {
            get
            {
                if (_dartyValue != 0) UpdateTabletMode();
                return _isTabletMode;
            }
        }


        private void UpdateTabletMode()
        {
            Interlocked.Exchange(ref _dartyValue, 0);

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell");
            if (regKey != null)
            {
                _isTabletMode = Convert.ToBoolean(regKey.GetValue("TabletMode", 0));
                regKey.Close();
            }

            Debug.WriteLine($"TabletMode: {_isTabletMode}");
        }

        // ウィンドウプロシージャ
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SETTINGCHANGE:
                    OnSettingChange(wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private void OnSettingChange(IntPtr wParam, IntPtr lParam)
        {
            string str = Marshal.PtrToStringAuto(lParam);
            if (str == "UserInteractionMode")
            {
                Interlocked.Exchange(ref _dartyValue, 1);
            }
        }
    }
}
