// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // マウスゼスチャ 方向
    public enum MouseGestureDirection
    {
        None,
        Up,
        Right,
        Down,
        Left
    }

    /// <summary>
    /// マウスゼスチャ シーケンス
    /// </summary>
    public class MouseGestureSequence : ObservableCollection<MouseGestureDirection>
    {
        // 記録用文字列に変換(U,D,L,Rの組み合わせ)
        public override string ToString()
        {
            string gestureText = "";
            foreach (var e in this)
            {
                gestureText += e.ToString()[0];
            }

            return gestureText;
        }

        //
        private static Dictionary<MouseGestureDirection, string> _DispStrings = new Dictionary<MouseGestureDirection, string>
        {
            [MouseGestureDirection.None] = "",
            [MouseGestureDirection.Up] = "↑",
            [MouseGestureDirection.Right] = "→",
            [MouseGestureDirection.Down] = "↓",
            [MouseGestureDirection.Left] = "←",
        };

        // 表示文字列に変換
        public string ToDispString()
        {
            string gestureText = "";
            foreach (var e in this)
            {
                gestureText += _DispStrings[e];
            }

            return gestureText;
        }
    }

}
