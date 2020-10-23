using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace NeeView.Runtime
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

        /// <summary>
        /// 最小化ボタンを無効
        /// </summary>
        /// <remarks>
        /// ウィンドウ生成前に実行する必要があります。
        /// </remarks>
        /// <param name="window"></param>
        public static void DisableMinimize(Window window)
        {
            if (window.IsLoaded) throw new InvalidOperationException();

            window.SourceInitialized += Window_SourceInitialized;

            void Window_SourceInitialized(object sender, EventArgs e)
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                var style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
                style = style & (~NativeMethods.WS_MINIMIZEBOX);
                NativeMethods.SetWindowLong(handle, NativeMethods.GWL_STYLE, style);
            }
        }
    }
}
