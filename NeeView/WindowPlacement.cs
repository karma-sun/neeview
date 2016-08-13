// Copyright(c) 2016 Mitsuhiro Ito(nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

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


    /// <summary>
    /// ウィンドウ状態
    /// </summary>
    public static class WindowPlacement
    {
        // memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public Win32Api.WINDOWPLACEMENT? WindowPlacement { get; set; }

            [DataMember]
            public Rect? WindowRect { get; set; }
        }

        // ウィンドウ状態反映
        public static void Restore(Window window, Memento memento, bool isNormalState)
        {
            if (memento?.WindowPlacement == null) return;

            // from http://grabacr.net/archives/1585
            var placement = (Win32Api.WINDOWPLACEMENT)memento.WindowPlacement;
            placement.length = Marshal.SizeOf(typeof(Win32Api.WINDOWPLACEMENT));
            placement.flags = 0;
            placement.showCmd = (isNormalState || placement.showCmd == Win32Api.SW.SHOWMINIMIZED) ? Win32Api.SW.SHOWNORMAL : placement.showCmd;

            var hwnd = new WindowInteropHelper(window).Handle;
            Win32Api.SetWindowPlacement(hwnd, ref placement);

            // スナップウィンドウサイズで復元 by nee
            if (memento.WindowRect != null &&  placement.showCmd == Win32Api.SW.SHOWNORMAL)
            {
                Rect rect = (Rect)memento.WindowRect;
                window.Left = rect.Left;
                window.Top = rect.Top;
                window.Width = rect.Width;
                window.Height = rect.Height;
            }
        }

        // ウィンドウ状態保存
        public static Memento CreateMemento(Window window)
        {
            // from http://grabacr.net/archives/1585
            Win32Api.WINDOWPLACEMENT placement;
            var hwnd = new WindowInteropHelper(window).Handle;
            Win32Api.GetWindowPlacement(hwnd, out placement);

            var windowRect = new Rect(window.Left, window.Top, window.Width, window.Height);

            var memento = new Memento();
            memento.WindowPlacement = placement;
            memento.WindowRect = windowRect;
            return memento;
        }

        // from http://grabacr.net/archives/1105
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
