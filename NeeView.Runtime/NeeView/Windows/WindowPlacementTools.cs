// from http://grabacr.net/archives/1585
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NeeView.Windows
{
    // TODO: AeroSnap保存ON/OFFフラグ。WindowPlacementOptionフラグ？
    public static class WindowPlacementTools
    {
        #region NativeApi

        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

            [DllImport("user32.dll")]
            public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);


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

                public int Length
                {
                    get => length;
                    set => length = value;
                }

                public int Flags
                {
                    get => flags;
                    set => flags = value;
                }

                public SW ShowCmd
                {
                    get => showCmd;
                    set => showCmd = value;
                }

                public POINT MinPosition
                {
                    get => minPosition;
                    set => minPosition = value;
                }

                public POINT MaxPosition
                {
                    get => maxPosition;
                    set => maxPosition = value;
                }

                public RECT NormalPosition
                {
                    get => normalPosition;
                    set => normalPosition = value;
                }

                public bool IsValid() => length == Marshal.SizeOf(typeof(WINDOWPLACEMENT));

                public override string ToString()
                {
                    return $"{ShowCmd},{MinPosition.X},{MinPosition.Y},{MaxPosition.X},{MaxPosition.Y},{NormalPosition.Left},{NormalPosition.Top},{NormalPosition.Right},{NormalPosition.Bottom}";
                }
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

                public override string ToString()
                {
                    return $"{X},{Y}";
                }

                public static POINT Parse(string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) throw new InvalidCastException();

                    var tokens = s.Split(',');
                    if (tokens.Length != 2) throw new InvalidCastException();

                    return new POINT(int.Parse(tokens[0]), int.Parse(tokens[1]));
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

                public int Width
                {
                    get => Right - Left;
                    set => Right = Left + value;
                }

                public int Height
                {
                    get => Bottom - Top;
                    set => Bottom = Top + value;
                }

                public override string ToString()
                {
                    return $"{Left},{Top},{Right},{Bottom}";
                }

                public static RECT Parse(string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) throw new InvalidCastException();

                    var tokens = s.Split(',');
                    if (tokens.Length != 4) throw new InvalidCastException();

                    return new RECT(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]));
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

        #endregion Native


        public static WindowPlacement StoreWindowPlacement(Window window, bool withAeroSnap)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) throw new InvalidOperationException();

            if (!(window is IDpiProvider dpiProvider)) throw new ArgumentException($"need window has IDpiProvider.");

            NativeMethods.GetWindowPlacement(hwnd, out NativeMethods.WINDOWPLACEMENT raw);
            ////Debug.WriteLine($"> Native.WindowPlacement: {raw}");

            if (withAeroSnap)
            {
                // AeroSnapの座標保存
                // NOTE: スナップ状態の復元方法が不明なため、現在のウィンドウサイズを通常ウィンドウサイズとして上書きする。
                NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT rect);
                Debug.WriteLine($"> Native.WindowRect: {rect}");

                if (raw.ShowCmd == NativeMethods.SW.SHOWNORMAL && !raw.NormalPosition.Equals(rect))
                {
                    Debug.WriteLine("> Window snapped, maybe.");
                    raw.NormalPosition = rect;
                }
            }

            // DPI補正
            // NOTE: WPFが復元時にDPIを加味してしまうようで、同じサイズにならない。このため、保存値からDPI要素を取り除いておく。
            // NOTE: 保存時に計算するのは、復元時ではWindowのDPIが取得できていないであることが予想されるため。
            var dpi = dpiProvider.Dpi;
            raw.normalPosition.Right = raw.normalPosition.Left + (int)(raw.normalPosition.Width / dpi.DpiScaleX + 0.5);
            raw.normalPosition.Bottom = raw.normalPosition.Top + (int)(raw.normalPosition.Height / dpi.DpiScaleY + 0.5);
            Debug.WriteLine($"> Store.WIDTH: {raw.normalPosition.Width}, DPI: {dpi.DpiScaleX}");

            return ConvertToWindowPlacement(raw);
        }


        public static void RestoreWindowPlacement(Window window, WindowPlacement placement)
        {
            if (placement == null || !placement.IsValid()) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            var raw = ConvertToNativeWindowPlacement(placement);

            NativeMethods.SetWindowPlacement(hwnd, ref raw);
        }


        private static WindowPlacement ConvertToWindowPlacement(NativeMethods.WINDOWPLACEMENT raw)
        {
            var memento = new WindowPlacement(
                ConvertToWindowState(raw.ShowCmd),
                raw.NormalPosition.Left,
                raw.NormalPosition.Top,
                raw.NormalPosition.Width,
                raw.NormalPosition.Height);
            return memento;
        }

        private static NativeMethods.WINDOWPLACEMENT ConvertToNativeWindowPlacement(WindowPlacement placement)
        {
            var raw = new NativeMethods.WINDOWPLACEMENT();
            raw.Length = Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));
            raw.Flags = 0;
            raw.ShowCmd = ConvertToNativeShowCmd(placement.WindowState);
            raw.MinPosition = new NativeMethods.POINT(-1, -1);
            raw.MaxPosition = new NativeMethods.POINT(-1, -1);
            raw.NormalPosition = new NativeMethods.RECT(placement.Left, placement.Top, placement.Right, placement.Bottom);
            return raw;
        }

        private static WindowState ConvertToWindowState(NativeMethods.SW showCmd)
        {
            switch (showCmd)
            {
                default:
                    return WindowState.Normal;
                case NativeMethods.SW.SHOWMINIMIZED:
                    return WindowState.Minimized;
                case NativeMethods.SW.SHOWMAXIMIZED:
                    return WindowState.Maximized;
            }
        }

        private static NativeMethods.SW ConvertToNativeShowCmd(WindowState windowState)
        {
            switch (windowState)
            {
                default:
                    return NativeMethods.SW.SHOWNORMAL;
                case WindowState.Minimized:
                    return NativeMethods.SW.SHOWMINIMIZED;
                case WindowState.Maximized:
                    return NativeMethods.SW.SHOWMAXIMIZED;
            }
        }
    }

}
