// from https://github.com/takanemu/WPFDragAndDropSample

// WIN32APIの高DPI対応
// from http://grabacr.net/archives/1105

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeeLaboratory.Windows
{
    /// <summary>
    /// マウスカーソル座標取得(WIN32)
    /// </summary>
    public static class CursorInfo
    {
        /// <summary>
        /// 現在のマウスカーソルのスクリーン座標を得ます
        /// </summary>
        /// <param name="pt"></param>
        [DllImport("user32.dll")]
        private static extern void GetCursorPos(out POINT pt);

        /// <summary>
        /// 画面上にある指定された点を、スクリーン座標からクライアント座標へ変換します。
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int ScreenToClient(IntPtr hwnd, ref POINT pt);

        /// <summary>
        /// 座標
        /// </summary>
        private struct POINT
        {
            public UInt32 X;
            public UInt32 Y;
        }

        /// <summary>
        /// 現在のマウスカーソル座標を取得
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public static Point GetNowPosition(Visual visual)
        {
            GetCursorPos(out POINT point);

            var source = HwndSource.FromVisual(visual) as HwndSource;
            var hwnd = source.Handle;

            ScreenToClient(hwnd, ref point);

            var dpiScaleFactor = GetDpiScaleFactor(visual);
            return new Point(point.X / dpiScaleFactor.X, point.Y / dpiScaleFactor.Y);
        }


        /// <summary>
        /// 現在の <see cref="T:System.Windows.Media.Visual"/> から、DPI 倍率を取得します。
        /// </summary>
        /// <returns>
        /// X 軸 および Y 軸それぞれの DPI 倍率を表す <see cref="T:System.Windows.Point"/>
        /// 構造体。取得に失敗した場合、(1.0, 1.0) を返します。
        /// </returns>
        public static Point GetDpiScaleFactor(System.Windows.Media.Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null && source.CompositionTarget != null)
            {
                return new Point(
                    source.CompositionTarget.TransformToDevice.M11,
                    source.CompositionTarget.TransformToDevice.M22);
            }

            return new Point(1.0, 1.0);
        }
    }
}
