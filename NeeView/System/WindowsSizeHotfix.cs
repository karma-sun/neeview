using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    /// <summary>
    /// ウィンドウ最大化時に、自動非表示のタスクバーが表示できない現象の修正
    /// </summary>
    /// <remarks>
    /// 完全な対処になっていない。モニターの解像度変更やタスクバーの位置に追従できていない。
    /// このため、タスクバーが非表示の場合に限り限定的に機能するようにしている。
    /// </remarks>
    public class WindowsSizeHotfix
    {
        internal static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;

                public POINT(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;

                public int Width
                {
                    get => right - left;
                    set => right = left + value;
                }

                public int Height
                {
                    get => bottom - top;
                    set => bottom = top + value;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MINMAXINFO
            {
                public POINT ptReserved;
                public POINT ptMaxSize;
                public POINT ptMaxPosition;
                public POINT ptMinTrackSize;
                public POINT ptMaxTrackSize;
            };


            public const int WM_GETMINMAXINFO = 0x0024;


            [DllImport("user32")]
            internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

            public const int MONITOR_DEFAULTTONULL = 0;
            public const int MONITOR_DEFAULTTOPRIMARY = 1;
            public const int MONITOR_DEFAULTTONEAREST = 2;


            [DllImport("user32")]
            internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MONITORINFO
            {
                public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                public RECT rcMonitor = new RECT();
                public RECT rcWork = new RECT();
                public int dwFlags = 0;
            }


            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            public const uint SWP_ASYNCWINDOWPOS = 0x4000;
            public const uint SWP_DEFERERASE = 0x2000;
            public const uint SWP_DRAWFRAME = 0x0020;
            public const uint SWP_FRAMECHANGED = 0x0020;
            public const uint SWP_HIDEWINDOW = 0x0080;
            public const uint SWP_NOACTIVATE = 0x0010;
            public const uint SWP_NOCOPYBITS = 0x0100;
            public const uint SWP_NOMOVE = 0x0002;
            public const uint SWP_NOOWNERZORDER = 0x0200;
            public const uint SWP_NOREDRAW = 0x0008;
            public const uint SWP_NOREPOSITION = 0x0200;
            public const uint SWP_NOSENDCHANGING = 0x0400;
            public const uint SWP_NOSIZE = 0x0001;
            public const uint SWP_NOZORDER = 0x0004;
            public const uint SWP_SHOWWINDOW = 0x0040;
        }



        private IntPtr _hWnd;


        public bool IsEnabled { get; set; } = true;


        public void Initialize(Window window)
        {
            if (window.IsLoaded)
            {
                AddHook(window);
            }
            else
            {
                window.Loaded += Window_Loaded;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var window = (Window)sender;

            AddHook(window);
            window.Loaded -= Window_Loaded;
        }

        private void AddHook(Window window)
        {
            _hWnd = new WindowInteropHelper(window).Handle;
            if (_hWnd == IntPtr.Zero)
            {
                return;
            }

            HwndSource.FromHwnd(_hWnd).AddHook(new HwndSourceHook(WindowProc));
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!this.IsEnabled)
            {
                return IntPtr.Zero;
            }

            switch (msg)
            {
                case NativeMethods.WM_GETMINMAXINFO:
                    handled = WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private bool WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            if (!TaskBarNativeTools.IsAutoHide())
            {
                return false;
            }

            var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero)
            {
                return false;
            }

            var monitorInfo = new NativeMethods.MONITORINFO();
            NativeMethods.GetMonitorInfo(monitor, monitorInfo);
            var workArea = monitorInfo.rcWork;
            var monitorArea = monitorInfo.rcMonitor;

            var mmi = (NativeMethods.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MINMAXINFO));
            mmi.ptMaxPosition.x = workArea.left - monitorArea.left;
            mmi.ptMaxPosition.y = workArea.top - monitorArea.top;
            mmi.ptMaxSize.x = Math.Abs(workArea.right - workArea.left);
            mmi.ptMaxSize.y = Math.Abs(workArea.bottom - workArea.top);
            var edge = GetAutoHideTaskbarEdge(monitor);
            mmi =AdjustMinMaxInfo(mmi, edge);
            Marshal.StructureToPtr(mmi, lParam, true);
            return true;
        }

        private NativeMethods.MINMAXINFO AdjustMinMaxInfo(NativeMethods.MINMAXINFO mmi, int edge)
        {
            switch (edge)
            {
                case TaskBarNativeTools.NativeMethods.ABE_LEFT:
                    mmi.ptMaxPosition.x += 2;
                    mmi.ptMaxTrackSize.x -= 2;
                    mmi.ptMaxSize.x -= 2;
                    break;
                case TaskBarNativeTools.NativeMethods.ABE_RIGHT:
                    mmi.ptMaxSize.x -= 2;
                    mmi.ptMaxTrackSize.x -= 2;
                    break;
                case TaskBarNativeTools.NativeMethods.ABE_TOP:
                    mmi.ptMaxPosition.y += 2;
                    mmi.ptMaxTrackSize.y -= 2;
                    mmi.ptMaxSize.y -= 2;
                    break;
                case TaskBarNativeTools.NativeMethods.ABE_BOTTOM:
                    mmi.ptMaxSize.y -= 2;
                    mmi.ptMaxTrackSize.y -= 2;
                    break;
            }
            return mmi;
        }

        private NativeMethods.RECT AdjustRect(NativeMethods.RECT rect, int edge)
        {
            switch (edge)
            {
                case TaskBarNativeTools.NativeMethods.ABE_LEFT:
                    rect.left += 2;
                    break;
                case TaskBarNativeTools.NativeMethods.ABE_RIGHT:
                    rect.right -= 2;
                    break;
                case TaskBarNativeTools.NativeMethods.ABE_TOP:
                    rect.top += 2;
                    break;
                case TaskBarNativeTools.NativeMethods.ABE_BOTTOM:
                    rect.bottom -= 2;
                    break;
            }
            return rect;
        }

        private int GetAutoHideTaskbarEdge(IntPtr monitor)
        {
            if (!TaskBarNativeTools.IsAutoHide())
            {
                return -1;
            }

            var primaryTaskBar = TaskBarNativeTools.GetPrimaryTaskBarHandle();
            if (primaryTaskBar == IntPtr.Zero)
            {
                return -1;
            }

            var primaryTaskBarMonitor = NativeMethods.MonitorFromWindow(primaryTaskBar, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (!monitor.Equals(primaryTaskBarMonitor))
            {
                return -1;
            }

            return TaskBarNativeTools.GetEdge();
        }

        public void SetMaximizedWindowPos()
        {
            if (_hWnd == IntPtr.Zero)
            {
                return;
            }

            var monitor = NativeMethods.MonitorFromWindow(_hWnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero)
            {
                return;
            }

            var monitorInfo = new NativeMethods.MONITORINFO();
            NativeMethods.GetMonitorInfo(monitor, monitorInfo);

            var edge = GetAutoHideTaskbarEdge(monitor);
            var rect = AdjustRect(monitorInfo.rcWork, edge);
            NativeMethods.SetWindowPos(_hWnd, IntPtr.Zero, rect.left, rect.top, rect.Width, rect.Height, NativeMethods.SWP_NOZORDER);
        }
    }
}
