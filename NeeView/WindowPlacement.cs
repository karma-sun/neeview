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
            public bool IsEnabled { get; set; }

            [DataMember]
            public WindowState WindowState { get; set; }

            [DataMember]
            public Rect WindowRect { get; set; }
        }

        // ウィンドウ状態反映
        public static void Restore(Window window, Memento memento)
        {
            if (memento == null || !memento.IsEnabled) return;

            Rect rect = memento.WindowRect;
            window.Left = rect.Left;
            window.Top = rect.Top;
            window.Width = rect.Width;
            window.Height = rect.Height;
            window.WindowState = (memento.WindowState == WindowState.Minimized) ? WindowState.Normal : memento.WindowState;
        }

        // ウィンドウ状態保存
        public static Memento CreateMemento(Window window)
        {
            var memento = new Memento();
            memento.WindowState = window.WindowState;
            memento.WindowRect = window.RestoreBounds;
            memento.IsEnabled = true;
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
