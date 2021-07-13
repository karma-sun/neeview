using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace NeeView
{
    public static class TaskBarNativeTools
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
            }


            public const uint ABE_LEFT = 0;
            public const uint ABE_TOP = 1;
            public const uint ABE_RIGHT = 2;
            public const uint ABE_BOTTOM = 3;

            public const int ABM_NEW = 0;
            public const int ABM_REMOVE = 1;
            public const int ABM_QUERYPOS = 2;
            public const int ABM_SETPOS = 3;
            public const int ABM_GETSTATE = 4;
            public const int ABM_GETTASKBARPOS = 5;
            public const int ABM_ACTIVATE = 6;
            public const int ABM_GETAUTOHIDEBAR = 7;
            public const int ABM_SETAUTOHIDEBAR = 8;
            public const int ABM_WINDOWPOSCHANGED = 9;
            public const int ABM_SETSTATE = 10;

            public const int ABS_AUTOHIDE = 1;
            public const int ABS_ALWAYSONTOP = 2;

            [StructLayout(LayoutKind.Sequential)]
            public struct APPBARDATA
            {
                public int cbSize;
                public IntPtr hWnd;
                public uint uCallbackMessage;
                public uint uEdge;
                public RECT rc;
                public int lParam;

                public static APPBARDATA Create()
                {
                    APPBARDATA appBarData = new APPBARDATA();
                    appBarData.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
                    return appBarData;
                }
            }

            [DllImport("shell32", CallingConvention = CallingConvention.StdCall)]
            internal static extern IntPtr SHAppBarMessage(uint dwMessage, [In] ref APPBARDATA pData);

            [DllImport("user32", SetLastError = true)]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }


        public static IntPtr GetPrimaryTaskBarHandle()
        {
            return NativeMethods.FindWindow("Shell_TrayWnd", null);
        }

        public static bool IsAutoHide()
        {
            var appBarData = NativeMethods.APPBARDATA.Create();
            var result = NativeMethods.SHAppBarMessage(NativeMethods.ABM_GETSTATE, ref appBarData);
            if (result.ToInt32() == NativeMethods.ABS_AUTOHIDE)
            {
                return true;
            }

            return false;
        }

        public static uint GetEdge()
        {
            var appBarData = NativeMethods.APPBARDATA.Create();
            var result = NativeMethods.SHAppBarMessage(NativeMethods.ABM_GETTASKBARPOS, ref appBarData);
            if (result != IntPtr.Zero)
            {
                return appBarData.uEdge;
            }

            return NativeMethods.ABE_BOTTOM;
        }
    }
}
