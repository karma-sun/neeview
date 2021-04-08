using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NeeView.Windows
{
    public static class WindowTools
    {
        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            public const int GWL_STYLE = -16;
            public const int WS_MAXIMIZEBOX = 0x00010000;
            public const int WS_MINIMIZEBOX = 0x00020000;
            public const int WS_SYSMENU = 0x00080000;


            [DllImport("user32.dll")]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            [DllImport("user32.dll")]
            public static extern uint TrackPopupMenuEx(IntPtr hMenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

            public const uint TPM_CENTERALIGN = 0x0004;
            public const uint TPM_LEFTALIGN = 0x0000;
            public const uint TPM_RIGHTALIGN = 0x0008;
            public const uint TPM_BOTTOMALIGN = 0x0020;
            public const uint TPM_TOPALIGN = 0x0008;
            public const uint TPM_VCENTERALIGN = 0x0010;
            public const uint TPM_NONOTIFY = 0x0080;
            public const uint TPM_RETURNCMD = 0x0100;
            public const uint TPM_LEFTBUTTON = 0x0000;
            public const uint TPM_RIGHTBUTTON = 0x0002;

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            public const uint WM_SYSCOMMAND = 0x0112;
        }


        [Flags]
        public enum WindowStyle
        {
            None = 0,

            // タイトルバー上にウィンドウメニューボックスを持つウィンドウを作成します。
            SystemMenu = NativeMethods.WS_SYSMENU,

            // 最小化ボタンを持つウィンドウを作成します。 WS_SYSMENU スタイルも指定する必要があります。拡張スタイルに WS_EX_CONTEXTHELP を指定することはできません。
            MinimizeBox = NativeMethods.WS_MINIMIZEBOX,

            MaximizeBox = NativeMethods.WS_MAXIMIZEBOX,
        }

        /// <summary>
        /// ウィンドウスタイルの一部無効化
        /// </summary>
        /// <param name="window"></param>
        /// <param name="disableStyleFlags">無効化するスタイル</param>
        public static void DisableStyle(Window window, WindowStyle disableStyleFlags)
        {
            if (window.IsLoaded)
            {
                UpdateSystemMenu();
            }
            else
            {
                window.SourceInitialized +=
                    (s, e) => UpdateSystemMenu();
            }

            void UpdateSystemMenu()
            { 
                var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                var style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
                style = style & (~(int)disableStyleFlags);
                NativeMethods.SetWindowLong(handle, NativeMethods.GWL_STYLE, style);
            }
        }

        /// <summary>
        /// システムメニューを表示
        /// </summary>
        /// <param name="window"></param>
        public static void ShowSystemMenu(Window window)
        {
            if (window is null) return;

            var hWnd = (new WindowInteropHelper(window)).Handle;
            if (hWnd == IntPtr.Zero) return;

            var hMenu = NativeMethods.GetSystemMenu(hWnd, false);
            if (hMenu == IntPtr.Zero) return;

            var screenPos = window.PointToScreen(Mouse.GetPosition(window));
            uint command = NativeMethods.TrackPopupMenuEx(hMenu, NativeMethods.TPM_LEFTBUTTON | NativeMethods.TPM_RETURNCMD, (int)screenPos.X, (int)screenPos.Y, hWnd, IntPtr.Zero);
            if (command == 0) return;

            NativeMethods.PostMessage(hWnd, NativeMethods.WM_SYSCOMMAND, new IntPtr(command), IntPtr.Zero);
        }

    }
}
