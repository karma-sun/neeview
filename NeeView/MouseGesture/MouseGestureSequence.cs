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
    // マウスジェスチャー 方向
    public enum MouseGestureDirection
    {
        None,
        Up,
        Right,
        Down,
        Left
    }

    /// <summary>
    /// マウスジェスチャー シーケンス
    /// </summary>
    public class MouseGestureSequence : ObservableCollection<MouseGestureDirection>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MouseGestureSequence()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gestureText">記録用文字列</param>
        public MouseGestureSequence(string gestureText)
        {
            if (!string.IsNullOrEmpty(gestureText))
            {
                foreach (char c in gestureText)
                {
                    MouseGestureDirection direction;
                    if (_Table.TryGetValue(c, out direction))
                    {
                        this.Add(direction);
                    }
                }
            }
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


        private static Dictionary<char, MouseGestureDirection> _Table = new Dictionary<char, MouseGestureDirection>
        {
            ['U'] = MouseGestureDirection.Up,
            ['R'] = MouseGestureDirection.Right,
            ['D'] = MouseGestureDirection.Down,
            ['L'] = MouseGestureDirection.Left,
        };
        

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


        // 表示文字列に変換(矢印の組み合わせ)
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
