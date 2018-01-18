// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
        UniformToSize, // 面積をウィンドウに合わせる
        UniformToVertical, // 高さをウィンドウに合わせる
    }

    public static class PageStretchModeExtension
    {
        public const string PageStretchMode_None = "オリジナルサイズ";
        public const string PageStretchMode_Inside = "大きい場合ウィンドウサイズに合わせる";
        public const string PageStretchMode_Outside = "小さい場合ウィンドウサイズに合わせる";
        public const string PageStretchMode_Uniform = "ウィンドウサイズに合わせる";
        public const string PageStretchMode_UniformToFill = "ウィンドウいっぱいに広げる";
        public const string PageStretchMode_UniformToSize = "面積をウィンドウに合わせる";
        public const string PageStretchMode_UniformToVertical = "高さをウィンドウに合わせる";

        // 表示名
        private static Dictionary<PageStretchMode, string> s_dispStrings = new Dictionary<PageStretchMode, string>
        {
            [PageStretchMode.None] = PageStretchMode_None,
            [PageStretchMode.Inside] = PageStretchMode_Inside,
            [PageStretchMode.Outside] = PageStretchMode_Outside,
            [PageStretchMode.Uniform] = PageStretchMode_Uniform,
            [PageStretchMode.UniformToFill] = PageStretchMode_UniformToFill,
            [PageStretchMode.UniformToSize] = PageStretchMode_UniformToSize,
            [PageStretchMode.UniformToVertical] = PageStretchMode_UniformToVertical,
        };

        // 表示名取得
        public static string ToDispString(this PageStretchMode mode)
        {
            return s_dispStrings[mode];
        }
    }
}
