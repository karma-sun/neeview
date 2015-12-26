// from http://grabacr.net/archives/1585

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    public static partial class Win32Api
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(            IntPtr hWnd,            [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(            IntPtr hWnd,            out WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public SW showCmd;
            public POINT minPosition;
            public POINT maxPosition;
            public RECT normalPosition;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        public enum SW
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10,
        }
    }

    [DataContract]
    public class WindowPlacement
    {
        [DataMember]
        Win32Api.WINDOWPLACEMENT? _WindowPlacement;

        public void Restore(Window window)
        {
            if (_WindowPlacement == null) return;

            var placement = (Win32Api.WINDOWPLACEMENT)_WindowPlacement;
            placement.length = Marshal.SizeOf(typeof(Win32Api.WINDOWPLACEMENT));
            placement.flags = 0;
            placement.showCmd = (placement.showCmd == Win32Api.SW.SHOWMINIMIZED) ? Win32Api.SW.SHOWNORMAL : placement.showCmd;

            var hwnd = new WindowInteropHelper(window).Handle;
            Win32Api.SetWindowPlacement(hwnd, ref placement);
        }

        public void Store(Window window)
        {
            Win32Api.WINDOWPLACEMENT placement;
            var hwnd = new WindowInteropHelper(window).Handle;
            Win32Api.GetWindowPlacement(hwnd, out placement);

            _WindowPlacement = placement;
        }
    }
}
