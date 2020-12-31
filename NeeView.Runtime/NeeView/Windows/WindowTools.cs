using System;
using System.Runtime.InteropServices;
using System.Windows;

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
            public const int WS_MINIMIZEBOX = 0x00020000;
            public const int WS_SYSMENU = 0x00080000;
        }

        [Flags]
        public enum WindowStyle
        {
            // タイトルバー上にウィンドウメニューボックスを持つウィンドウを作成します。
            SystemMenu = NativeMethods.WS_SYSMENU,

            // 最小化ボタンを持つウィンドウを作成します。 WS_SYSMENU スタイルも指定する必要があります。拡張スタイルに WS_EX_CONTEXTHELP を指定することはできません。
            MinimizeBox = NativeMethods.WS_MINIMIZEBOX,
        }

        /// <summary>
        /// ウィンドウスタイルの一部無効化
        /// </summary>
        /// <remarks>
        /// ウィンドウ生成前に実行する必要があります。
        /// </remarks>
        /// <param name="window"></param>
        /// <param name="disableStyleFlags">無効化するスタイル</param>
        public static void DisableStyle(Window window, WindowStyle disableStyleFlags)
        {
            if (window.IsLoaded) throw new InvalidOperationException();

            window.SourceInitialized += Window_SourceInitialized;

            void Window_SourceInitialized(object sender, EventArgs e)
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                var style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
                style = style & (~(int)disableStyleFlags);
                NativeMethods.SetWindowLong(handle, NativeMethods.GWL_STYLE, style);
            }
        }
    }
}
