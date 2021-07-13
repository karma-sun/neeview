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

                public int GetWidth() => right - left;
                public int GetHeight() => bottom - top;
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

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MONITORINFO
            {
                public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                public RECT rcMonitor = new RECT();
                public RECT rcWork = new RECT();
                public int dwFlags = 0;
            }

            public const int MONITOR_DEFAULTTONULL = 0;
            public const int MONITOR_DEFAULTTOPRIMARY = 1;
            public const int MONITOR_DEFAULTTONEAREST = 2;

            [DllImport("user32")]
            internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

            [DllImport("user32")]
            internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
        }


        private const int WM_GETMINMAXINFO = 0x0024;


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
            var handle = new WindowInteropHelper(window).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!this.IsEnabled)
            {
                return IntPtr.Zero;
            }

            switch (msg)
            {
                case WM_GETMINMAXINFO:
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
            mmi = AdjustWorkingAreaForAutoHide(mmi, monitor);
            Marshal.StructureToPtr(mmi, lParam, true);
            return true;
        }

        private NativeMethods.MINMAXINFO AdjustWorkingAreaForAutoHide(NativeMethods.MINMAXINFO mmi, IntPtr monitor)
        {
            var primaryTaskBar = TaskBarNativeTools.GetPrimaryTaskBarHandle();
            if (primaryTaskBar == IntPtr.Zero)
            {
                return mmi;
            }

            var primaryTaskBarMonitor = NativeMethods.MonitorFromWindow(primaryTaskBar, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (!monitor.Equals(primaryTaskBarMonitor))
            {
                return mmi;
            }

            if (!TaskBarNativeTools.IsAutoHide())
            {
                return mmi;
            }

            var edge = TaskBarNativeTools.GetEdge();
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

    }
}
