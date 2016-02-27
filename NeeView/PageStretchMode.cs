// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 画像のストレッチモード
    public enum PageStretchMode
    {
        None, // もとの大きさ
        Inside,  // もとの大きさ、大きい場合はウィンドウサイズに合わせる
        Outside, // もとの大きさ、小さい場合はウィンドウサイズに合わせる
        Uniform, // ウィンドウサイズに合わせる
        UniformToFill, // ウィンドウいっぱいに広げる
        UniformToVertical, // 高さをウィンドウに合わせる
    }

    public static class PageStretchModeExtension
    {
        private static Dictionary<PageStretchMode, CommandType> _CommandTable = new Dictionary<PageStretchMode, CommandType>
        {
            [PageStretchMode.None] = CommandType.SetStretchModeNone,
            [PageStretchMode.Inside] = CommandType.SetStretchModeInside,
            [PageStretchMode.Outside] = CommandType.SetStretchModeOutside,
            [PageStretchMode.Uniform] = CommandType.SetStretchModeUniform,
            [PageStretchMode.UniformToFill] = CommandType.SetStretchModeUniformToFill,
            [PageStretchMode.UniformToVertical] = CommandType.SetStretchModeUniformToVertical,
        };

        public static bool IsEnabled(this PageStretchMode mode)
        {
            return ModelContext.CommandTable[_CommandTable[mode]].IsToggled;
        }


        // トグル
        public static PageStretchMode GetToggle(this PageStretchMode mode)
        {
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                mode = (PageStretchMode)(((int)mode + 1) % length);
            }
            while (!mode.IsEnabled() && count++ < length);
            return mode;
        }

        // 逆トグル
        public static PageStretchMode GetToggleReverse(this PageStretchMode mode)
        {
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                mode = (PageStretchMode)(((int)mode + length - 1) % length);
            }
            while (!mode.IsEnabled() && count++ < length);
            return mode;
        }

        // 表示名
        private static Dictionary<PageStretchMode, string> _DispStrings = new Dictionary<PageStretchMode, string>
        {
            [PageStretchMode.None] = "オリジナルサイズ",
            [PageStretchMode.Inside] = "大きい場合ウィンドウサイズに合わせる",
            [PageStretchMode.Outside] = "小さい場合ウィンドウサイズに合わせる",
            [PageStretchMode.Uniform] = "ウィンドウサイズに合わせる",
            [PageStretchMode.UniformToFill] = "ウィンドウいっぱいに広げる",
            [PageStretchMode.UniformToVertical] = "高さをウィンドウに合わせる",
        };

        // 表示名取得
        public static string ToDispString(this PageStretchMode mode)
        {
            return _DispStrings[mode];
        }
    }

}
